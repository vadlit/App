namespace DotNetRu.DataStore.Audit.Extensions
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using DotNetRu.DataStore.Audit.Models;
    using DotNetRu.DataStore.Audit.RealmModels;

    public static class SpeakerExtensions
    {
        public static SpeakerModel ToModel(this Speaker speaker)
        {
            var speakerAvatarBytes = (byte[])speaker.Avatar.Clone();

            return new SpeakerModel
                       {
                           Id = speaker.Id,
                           FirstName = speaker.Name,
                           LastName = string.Empty,
                           CompanyName = speaker.CompanyName,
                           CompanyWebsiteUrl = speaker.CompanyUrl,
                           TwitterUrl = speaker.TwitterUrl,
                           BlogUrl = speaker.BlogUrl,
                           Biography = speaker.Description,
                           Avatar = new CustomStreamImageSource
                                        {
                                            Key = speaker.Id,
                                            Stream = token => Task.FromResult<Stream>(new MemoryStream(speakerAvatarBytes))
                                        },
                           Talks = speaker.Talks.ToList().Select(x => x.ToModel())
                       };
        }
    }
}