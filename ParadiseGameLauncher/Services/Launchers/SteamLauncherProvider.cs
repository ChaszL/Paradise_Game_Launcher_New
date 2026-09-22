using Microsoft.Win32;
using ParadiseGameLauncher.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParadiseGameLauncher.Services.Launchers
{
    // SteamLauncherProvider is scanning for games and returns them as GameData objects.
    // It implements the ILauncherProvider interface.
    public class SteamLauncherProvider : ILauncherProvider
    {
        // deafults the source name to "Steam" for all games found by this provider
        public string SourceName => "Steam";

        // scans for installed games from the Steam launcher and add them to the results.
        public Task<List<GameData>> ScanAsync()
        {
            var results = new List<GameData>();

            var steamAppsFolders = GetSteamAppsFolders();
            if (steamAppsFolders.Count == 0)
                return Task.FromResult(results);

            foreach (var steamapps in steamAppsFolders)
            {
                if (!Directory.Exists(steamapps))
                    continue;

                foreach (var manifestFile in Directory.EnumerateFiles(steamapps, "appmanifest*.acf"))
                {
                    try
                    {
                        var game = ParseManifest(manifestFile, steamapps);
                        if (game != null)
                            results.Add(game);
                    }
                    catch
                    {
                        
                    }
                }
            }

            // return happens ONCE, after every folder has been checked
            return Task.FromResult(results);
        }

        // parses the Steam app manifest file to extract game information and returns a GameData object.
        private GameData? ParseManifest(string manifestPath, string steamappsFolder)
        {
            var text = File.ReadAllText(manifestPath);

            var appID = ExtractVdfValue(text, "appid");
            var name = ExtractVdfValue(text, "name");
            var installDir = ExtractVdfValue(text, "installdir");

            if (string.IsNullOrWhiteSpace(appID) || string.IsNullOrWhiteSpace(installDir))
                return null;

            var fullInstallPath = Path.Combine(steamappsFolder, "common", installDir);
            if (!Directory.Exists(fullInstallPath))
                return null;

            return new GameData
            {
                Id = $"steam_{appID}",
                Name = string.IsNullOrWhiteSpace(name) ? installDir : name,
                Path = $"steam://run/{appID}",
                Category = "Uncategorized",
                IsInstalled = true,
                SourceLauncher = SourceName
            };
        }

        // extracts the app id from the steam games manifiest file (returns string or null if not found)
        private static string? ExtractVdfValue(string text, string key)
        {
            var match = Regex.Match(text, $"\"{key}\"\\s*\"([^\"]*)\"", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        // retrieves the Steam installation folders from the registry and libraryfolders.vdf files
        // returning a list of steamapps directories.
        private static List<string> GetSteamAppsFolders()
        {
            var resultsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
                var steamPath = key?.GetValue("SteamPath") as string;
                if (!string.IsNullOrWhiteSpace(steamPath))
                    resultsSet.Add(Path.Combine(steamPath, "steamapps"));
            }
            catch { }

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey("Software\\WOW6432Node\\Valve\\Steam");
                var steamPath = key?.GetValue("InstallPath") as string;
                if (!string.IsNullOrWhiteSpace(steamPath))
                    resultsSet.Add(Path.Combine(steamPath, "steamapps"));
            }
            catch { }

            var toAdd = new List<string>();
            foreach (var steamapps in resultsSet.ToList())
            {
                try
                {
                    var vdf = Path.Combine(steamapps, "libraryfolders.vdf");
                    if (File.Exists(vdf))  
                    {
                        foreach (var lib in ParseLibraryFoldersVdf(vdf))
                            toAdd.Add(Path.Combine(lib, "steamapps"));
                    }
                }
                catch { }
            }

            foreach (var a in toAdd)
                resultsSet.Add(a);

            return resultsSet.ToList();  
        }

        // parses the Steam libraryfolders.vdf file to extract additional Steam library paths
        private static List<string> ParseLibraryFoldersVdf(string vdfPath)
        {
            var results = new List<string>();
            try
            {
                var text = File.ReadAllText(vdfPath);
                var matches = Regex.Matches(text, "\"path\"\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
                foreach (Match m in matches)
                {
                    var p = m.Groups[1].Value.Replace("\\\\", "\\");
                    if (!string.IsNullOrWhiteSpace(p)) results.Add(p);
                }
            }
            catch { }
            return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}