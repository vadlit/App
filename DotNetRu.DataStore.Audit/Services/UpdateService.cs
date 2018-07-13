﻿namespace DotNetRu.DataStore.Audit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    using AutoMapper;

    using DotNetRu.DataStore.Audit.RealmModels;
    using DotNetRu.DataStore.Audit.XmlEntities;
    using DotNetRu.Utils;

    using Octokit;

    using Realms;

    public static class UpdateService
    {
        private const int DotNetRuAppRepositoryID = 89862917;

        public static async Task UpdateAudit()
        {
            try
            {
                DotNetRuLogger.TrackEvent("AuditUpdate. Started updating audit");

                string currentCommitSha;
                using (var auditRealm = Realm.GetInstance("Audit.realm"))
                {
                    var auditVersion = auditRealm.All<AuditVersion>().Single();
                    currentCommitSha = auditVersion.CommitHash;
                }

                DotNetRuLogger.TrackEvent("AuditUpdate. Current synchronization context: " + SynchronizationContext.Current);
                DotNetRuLogger.TrackEvent("AuditUpdate. Current version is: " + currentCommitSha);

                var client = new GitHubClient(new ProductHeaderValue("DotNetRu"));

                var reference = await client.Git.Reference.Get(DotNetRuAppRepositoryID, "heads/master");
                var latestMasterCommitSha = reference.Object.Sha;

                DotNetRuLogger.TrackEvent("AuditUpdate. Latest version (master) is: " + latestMasterCommitSha);

                var contentUpdate = await client.Repository.Commit.Compare(
                                        DotNetRuAppRepositoryID,
                                        currentCommitSha,
                                        latestMasterCommitSha);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var httpClient = new HttpClient();

                var streamTasks = contentUpdate.Files.Select(
                    async file => new UpdatedFile
                                      {
                                          Filename = file.Filename,
                                          Content = await httpClient.GetByteArrayAsync(file.RawUrl)
                                                        .ConfigureAwait(false)
                                      });
                var fileContents = await Task.WhenAll(streamTasks);

                DotNetRuLogger.TrackEvent("AuditUpdate. Downloading files time: " + stopwatch.Elapsed.ToString("g"));

                var xmlFiles = fileContents.Where(x => x.Filename.EndsWith(".xml")).ToList();

                using (var auditRealm = Realm.GetInstance("Audit.realm"))
                {
                    using (var trans = auditRealm.BeginWrite())
                    {
                        var mapper = GetAutoMapper(auditRealm);

                        UpdateModels<SpeakerEntity>(mapper, auditRealm, xmlFiles, "speakers");
                        UpdateModels<FriendEntity>(mapper, auditRealm, xmlFiles, "friends");
                        UpdateModels<VenueEntity>(mapper, auditRealm, xmlFiles, "venues");
                        UpdateModels<TalkEntity>(mapper, auditRealm, xmlFiles, "talks");
                        UpdateModels<MeetupEntity>(mapper, auditRealm, xmlFiles, "meetups");

                        var speakerPhotos = fileContents.Where(x => x.Filename.EndsWith("avatar.jpg"));
                        UpdateSpeakerAvatars(auditRealm, speakerPhotos);

                        var auditVersion = auditRealm.All<AuditVersion>().Single();

                        auditVersion.CommitHash = latestMasterCommitSha;
                        auditRealm.Add(auditVersion, update: true);

                        trans.Commit();
                    }
                }

                stopwatch.Stop();

                DotNetRuLogger.TrackEvent("AuditUpdate. Finished! Time: " + stopwatch.Elapsed.ToString("g"));
            }
            catch (Exception e)
            {
                DotNetRuLogger.Report(e, "AuditUpdate");

                throw;
            }
        }

        private static void UpdateSpeakerAvatars(Realm auditRealm, IEnumerable<UpdatedFile> speakerPhotos)
        {
            foreach (UpdatedFile updatedFile in speakerPhotos)
            {
                var speakerID = updatedFile.Filename.Split('/')[2];

                byte[] speakerAvatar = updatedFile.Content;

                var speaker = auditRealm.Find<Speaker>(speakerID);
                speaker.Avatar = speakerAvatar;

                DotNetRuLogger.TrackEvent("AuditUpdate. Updated speaker avatar: " + updatedFile.Filename);
            }
        }

        private static void UpdateModels<T>(IMapper mapper, Realm auditRealm, IEnumerable<UpdatedFile> xmlFiles, string entityName)
        {
            var newEntities = xmlFiles.Where(x => x.Filename.StartsWith("db/" + entityName));
            UpdateModels<T>(mapper, auditRealm, newEntities);
        }

        private static void UpdateModels<T>(IMapper mapper, Realm auditRealm, IEnumerable<UpdatedFile> files)
        {
            foreach (UpdatedFile file in files)
            {
                using (var memoryStream = new MemoryStream(file.Content))
                {
                    var xmlEntity = new XmlSerializer(typeof(T)).Deserialize(memoryStream);

                    DotNetRuLogger.TrackEvent($"AuditUpdate: updating {file.Filename}");

                    var realmType = mapper.ConfigurationProvider.GetAllTypeMaps().Single(x => x.SourceType == typeof(T))
                        .DestinationType;

                    var realmObject = mapper.Map(xmlEntity, typeof(T), realmType);

                    auditRealm.Add(realmObject as RealmObject, update: true);

                    DotNetRuLogger.TrackEvent($"AuditUpdate. Updated {file.Filename}");
                }
            }
        }

        private static IMapper GetAutoMapper(Realm auditRealm)
        {
            var config = new MapperConfiguration(
                cfg =>
                    {
                        cfg.CreateMap<SpeakerEntity, Speaker>().AfterMap(
                            (src, dest) =>
                                {
                                    var speakerID = src.Id;
                                    var existingSpeaker = auditRealm.Find<Speaker>(speakerID);
                                    if (existingSpeaker != null)
                                    {
                                        dest.Avatar = existingSpeaker.Avatar;
                                    }
                                });
                        cfg.CreateMap<VenueEntity, Venue>();
                        cfg.CreateMap<FriendEntity, Friend>();
                        cfg.CreateMap<CommunityEntity, Community>();
                        cfg.CreateMap<TalkEntity, Talk>().AfterMap(
                            (src, dest) =>
                                {
                                    foreach (string speakerId in src.SpeakerIds)
                                    {
                                        var speaker = auditRealm.Find<Speaker>(speakerId);

                                        dest.Speakers.Add(speaker);
                                    }
                                });
                        cfg.CreateMap<SessionEntity, Session>().AfterMap(
                            (src, dest) =>
                                {
                                    dest.Talk = auditRealm.Find<Talk>(src.TalkId);
                                });
                        cfg.CreateMap<MeetupEntity, Meetup>()
                            .ForMember(
                                dest => dest.Sessions,
                                o => o.ResolveUsing((src, dest, sessions, context) => context.Mapper.Map(src.Sessions, dest.Sessions)))
                            .AfterMap(
                                (src, dest) =>
                                    {
                                        foreach (string friendId in src.FriendIds)
                                        {
                                            var friend = auditRealm.Find<Friend>(friendId);
                                            dest.Friends.Add(friend);
                                        }

                                        dest.Venue = auditRealm.Find<Venue>(src.VenueId);
                                    });
                    });

            return config.CreateMapper();            
        }

        private class UpdatedFile
        {
            public string Filename { get; set; }

            public byte[] Content { get; set; }
        }
    }
}