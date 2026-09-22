using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParadiseGameLauncher.Data
{
    public class AppSettings
    {
        // object to hold API keys, Shortcut fallback path, and Onboarding status
        public string SteamGridApiKey { get; set; } = string.Empty;
        public string GameLibraryPath { get; set; } = string.Empty;
        public bool AutoLocateGames { get; set; } = true;
        public bool CompletedOnboarding { get; set; } = false;

        // object to hold UI settings for the app (All Games)
        public int GamesBannerWidth { get; set; } = 150;
        public int GamesBannerHeight { get; set; } = 225;


        // object to hold UI settings for the app (Recent Games)
        public double RecentsBannerWidth { get; set; } = 80;
        public double RecentsBannerHeight { get; set; } = 120;

        // object to hold UI settings for the app (Overall App Colors)
        public string BackgroundColor { get; set; } = "#121212";
        public string NavbarColor { get; set; } = "#1e1e1e";
        public string LogoColor { get; set; } = "#ffffff";
        public string TextColor { get; set; } = "#ffffff";

        // Gets the base data directory for the application to store application
        // data such as settings and game records into json files.
        public static readonly string BaseDataDirectory = GetBaseDataDirectory();

        private static string GetBaseDataDirectory()
        {
            try
            {
                var _ = Windows.ApplicationModel.Package.Current;
                return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            }
            catch
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }
    }
}
