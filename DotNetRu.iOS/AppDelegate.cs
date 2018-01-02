﻿using DotNetRu.Clients.Portable.Model;
using DotNetRu.Utils.Helpers;
using DotNetRu.Utils.Interfaces;

namespace XamarinEvolve.iOS
{
    using System;
    using System.Collections.Generic;

    using CoreSpotlight;

    using FormsToolkit;
    using FormsToolkit.iOS;

    using Foundation;

    using Google.AppIndexing;

    using ImageCircle.Forms.Plugin.iOS;

    using Plugin.Share;

    using Refractored.XamForms.PullToRefresh.iOS;

    using Social;

    using UIKit;

    using Xamarin.Forms;
    using Xamarin.Forms.Platform.iOS;

    using Clients.Portable;

    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public static class ShortcutIdentifier
        {
            public const string Tweet = AboutThisApp.PackageName + ".tweet";

            public const string Announcements = AboutThisApp.PackageName + ".announcements";

            public const string Events = AboutThisApp.PackageName + ".events";
        }

        internal static UIColor PrimaryColor;

        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            // Code for starting up the Xamarin Test Cloud Agent
#if ENABLE_TEST_CLOUD
            //Calabash.Start();

            //// Mapping StyleId to iOS Labels
            //Forms.ViewInitialized += (sender, e) =>
            //    {
            //        if (null != e.View.StyleId)
            //        {
            //            e.NativeView.AccessibilityIdentifier = e.View.StyleId;
            //        }
            //    };
#endif

            Forms.Init();

            this.SetMinimumBackgroundFetchInterval();

            InitializeDependencies();

            this.LoadApplication(new DotNetRu.Clients.UI.App());

            InitializeThemeColors();

            // Process any potential notification data from launch
            this.ProcessNotification(launchOptions);

            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidBecomeActiveNotification, this.DidBecomeActive);

            // Get possible shortcut item
            if (launchOptions != null)
            {
                this.LaunchedShortcutItem =
                    launchOptions[UIApplication.LaunchOptionsShortcutItemKey] as UIApplicationShortcutItem;
            }

            return base.FinishedLaunching(uiApplication, launchOptions); // && shouldPerformAdditionalDelegateHandling;
        }

        void DidBecomeActive(NSNotification notification)
        {
            ((DotNetRu.Clients.UI.App)Xamarin.Forms.Application.Current).SecondOnResume();
        }

        static void InitializeDependencies()
        {
            Toolkit.Init();

            AppIndexing.SharedInstance.RegisterApp(PublicationSettings.iTunesAppId);

            ShareImplementation.ExcludedUIActivityTypes = new List<NSString>
                                                                           {
                                                                               UIActivityType
                                                                                   .PostToFacebook,
                                                                               UIActivityType
                                                                                   .AssignToContact,
                                                                               UIActivityType
                                                                                   .OpenInIBooks,
                                                                               UIActivityType
                                                                                   .PostToVimeo,
                                                                               UIActivityType
                                                                                   .PostToFlickr,
                                                                               UIActivityType
                                                                                   .SaveToCameraRoll
                                                                           };
            ImageCircleRenderer.Init();
            NonScrollableListViewRenderer.Initialize();
            SelectedTabPageRenderer.Initialize();
            TextViewValue1Renderer.Init();
            PullToRefreshLayoutRenderer.Init();
        }

        private static void InitializeThemeColors()
        {
            // Set up appearance after loading theme resources in App.xaml
            PrimaryColor = ((Color)Xamarin.Forms.Application.Current.Resources["Primary"]).ToUIColor();
            UINavigationBar.Appearance.BarTintColor =
                ((Color)Xamarin.Forms.Application.Current.Resources["BarBackgroundColor"]).ToUIColor();
            UINavigationBar.Appearance.TintColor = PrimaryColor; // Tint color of button items
            UIBarButtonItem.Appearance.TintColor = PrimaryColor; // Tint color of button items
            UITabBar.Appearance.TintColor = PrimaryColor;
            UISwitch.Appearance.OnTintColor = PrimaryColor;
            UIAlertView.Appearance.TintColor = PrimaryColor;

            UIView.AppearanceWhenContainedIn(typeof(UIAlertController)).TintColor = PrimaryColor;
            UIView.AppearanceWhenContainedIn(typeof(UIActivityViewController)).TintColor = PrimaryColor;
            UIView.AppearanceWhenContainedIn(typeof(SLComposeViewController)).TintColor = PrimaryColor;
        }

        public override void WillEnterForeground(UIApplication uiApplication)
        {
            base.WillEnterForeground(uiApplication);
            ((DotNetRu.Clients.UI.App)Xamarin.Forms.Application.Current).SecondOnResume();
        }

        public override void RegisteredForRemoteNotifications(UIApplication app, NSData deviceToken)
        {
            // #if ENABLE_TEST_CLOUD

            // #else

            // if (ApiKeys.AzureServiceBusUrl == nameof(ApiKeys.AzureServiceBusUrl))
            // return;

            // // Connection string from your azure dashboard
            // var cs = SBConnectionString.CreateListenAccess(
            // new NSUrl(ApiKeys.AzureServiceBusUrl),
            // ApiKeys.AzureKey);

            // // Register our info with Azure
            // var hub = new SBNotificationHub (cs, ApiKeys.AzureHubName);
            // hub.RegisterNativeAsync (deviceToken, null, err => {
            // if (err != null)
            // Console.WriteLine("Error: " + err.Description);
            // else
            // Console.WriteLine("Success");
            // });
            // #endif
        }

        public override void ReceivedRemoteNotification(UIApplication app, NSDictionary userInfo)
        {
            // Process a notification received while the app was already open
            this.ProcessNotification(userInfo);
        }

        public override bool HandleOpenURL(UIApplication application, NSUrl url)
        {
            if (!string.IsNullOrEmpty(url.AbsoluteString))
            {
                ((DotNetRu.Clients.UI.App)Xamarin.Forms.Application.Current).SendOnAppLinkRequestReceived(new Uri(url.AbsoluteString));
                return true;
            }

            return false;
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (!string.IsNullOrEmpty(url.AbsoluteString))
            {
                ((DotNetRu.Clients.UI.App)Xamarin.Forms.Application.Current).SendOnAppLinkRequestReceived(new Uri(url.AbsoluteString));
                return true;
            }

            return false;
        }

        public override bool OpenUrl(
            UIApplication application,
            NSUrl url,
            string sourceApplication,
            NSObject annotation)
        {
            if (!string.IsNullOrEmpty(url.AbsoluteString))
            {
                ((DotNetRu.Clients.UI.App)Xamarin.Forms.Application.Current).SendOnAppLinkRequestReceived(new Uri(url.AbsoluteString));
                return true;
            }

            return false;
        }

        private void ProcessNotification(NSDictionary userInfo)
        {
            if (userInfo == null)
            {
                return;
            }

            Console.WriteLine("Received Notification");

            var apsKey = new NSString("aps");

            if (userInfo.ContainsKey(apsKey))
            {
                var alertKey = new NSString("alert");

                var aps = (NSDictionary)userInfo.ObjectForKey(apsKey);

                if (aps.ContainsKey(alertKey))
                {
                    var alert = (NSString)aps.ObjectForKey(alertKey);

                    try
                    {

                        var avAlert = new UIAlertView($"{AboutThisApp.AppName} Update", alert, null, "OK", null);
                        avAlert.Show();

                    }
                    catch (Exception ex)
                    {

                    }

                    Console.WriteLine("Notification: " + alert);
                }
            }
        }

        #region Quick Action

        public UIApplicationShortcutItem LaunchedShortcutItem { get; set; }

        public override void OnActivated(UIApplication uiApplication)
        {
            Console.WriteLine("OnActivated");

            // Handle any shortcut item being selected
            this.HandleShortcutItem(this.LaunchedShortcutItem);



            // Clear shortcut after it's been handled
            this.LaunchedShortcutItem = null;
        }

        void CheckForAppLink(NSUserActivity userActivity)
        {
            var link = string.Empty;

            switch (userActivity.ActivityType)
            {
                case "NSUserActivityTypeBrowsingWeb":
                    link = userActivity.WebPageUrl.AbsoluteString;
                    break;
                case "com.apple.corespotlightitem":
                    if (userActivity.UserInfo.ContainsKey(CSSearchableItem.ActivityIdentifier))
                        link = userActivity.UserInfo.ObjectForKey(CSSearchableItem.ActivityIdentifier).ToString();
                    break;
                default:
                    if (userActivity.UserInfo.ContainsKey(new NSString("link")))
                        link = userActivity.UserInfo[new NSString("link")].ToString();
                    break;
            }

            if (!string.IsNullOrEmpty(link))
                ((DotNetRu.Clients.UI.App)Xamarin.Forms.Application.Current).SendOnAppLinkRequestReceived(new Uri(link));
        }

        // if app is already running
        public override void PerformActionForShortcutItem(
            UIApplication application,
            UIApplicationShortcutItem shortcutItem,
            UIOperationHandler completionHandler)
        {
            Console.WriteLine("PerformActionForShortcutItem");

            // Perform action
            var handled = this.HandleShortcutItem(shortcutItem);
            completionHandler(handled);
        }

        public bool HandleShortcutItem(UIApplicationShortcutItem shortcutItem)
        {
            Console.WriteLine("HandleShortcutItem ");
            var handled = false;

            // Anything to process?
            if (shortcutItem == null) return false;

            // Take action based on the shortcut type
            switch (shortcutItem.Type)
            {
                case ShortcutIdentifier.Tweet:
                    Console.WriteLine("QUICKACTION: Tweet");
                    var slComposer = SLComposeViewController.FromService(SLServiceType.Twitter);
                    if (slComposer == null)
                    {
                        new UIAlertView(
                            title: "Unavailable",
                            message: "Twitter is not available, please sign in on your devices settings screen.",
                            del: null,
                            cancelButtonTitle: "OK").Show();
                    }
                    else
                    {
                        slComposer.SetInitialText(EventInfo.HashTag);
                        if (slComposer.EditButtonItem != null)
                        {
                            slComposer.EditButtonItem.TintColor = PrimaryColor;
                        }

                        slComposer.CompletionHandler += result =>
                            {
                                this.InvokeOnMainThread(
                                    () => UIApplication.SharedApplication.KeyWindow.RootViewController
                                        .DismissViewController(true, null));
                            };

                        UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewControllerAsync(
                            slComposer,
                            true);
                    }

                    handled = true;
                    break;
                case ShortcutIdentifier.Announcements:
                    Console.WriteLine("QUICKACTION: Accouncements");
                    this.ContinueNavigation(AppPage.Notification);
                    handled = true;
                    break;
                case ShortcutIdentifier.Events:
                    Console.WriteLine("QUICKACTION: Meetups");
                    this.ContinueNavigation(AppPage.Meetups);
                    handled = true;
                    break;
            }

            Console.Write(handled);

            // Return results
            return handled;
        }

        void ContinueNavigation(AppPage page, string id = null)
        {
            Console.WriteLine("ContinueNavigation");

            // TODO: display UI in Forms somehow
            Console.WriteLine("Show the page for " + page);
            MessagingService.Current.SendMessage(
                "DeepLinkPage",
                new DeepLinkPage { Page = page, Id = id });
        }

        public override void UserActivityUpdated(UIApplication application, NSUserActivity userActivity)
        {
            this.CheckForAppLink(userActivity);
        }

        public override bool ContinueUserActivity(
            UIApplication application,
            NSUserActivity userActivity,
            UIApplicationRestorationHandler completionHandler)
        {
            this.CheckForAppLink(userActivity);
            return true;
        }

        #endregion

        #region Background Refresh

        private void SetMinimumBackgroundFetchInterval()
        {
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(MINIMUM_BACKGROUND_FETCH_INTERVAL);
        }

        // Minimum number of seconds between a background refresh this is shorter than Android because it is easily killed off.
        // 20 minutes = 20 * 60 = 1200 seconds
        private const double MINIMUM_BACKGROUND_FETCH_INTERVAL = 1200;

        // Called whenever your app performs a background fetch
        public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            // Do Background Fetch
            var downloadSuccessful = false;
            var logger = DependencyService.Get<ILogger>();

            try
            {
                Forms.Init(); // need for dependency services
            }
            catch (Exception ex)
            {
                ex.Data["Method"] = "PerformFetch";
                logger.Report(ex);
            }

            // If you don't call this, your application will be terminated by the OS.
            // Allows OS to collect stats like data cost and power consumption
            var resultFlag = downloadSuccessful ? UIBackgroundFetchResult.NewData : UIBackgroundFetchResult.Failed;
            completionHandler(resultFlag);
        }

        #endregion
    }
}

