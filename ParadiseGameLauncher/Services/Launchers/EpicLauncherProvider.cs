using ParadiseGameLauncher.Models;
using ParadiseGameLauncher.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ParadiseGameLauncher.Services.Launchers
{
    // Finds games installed via the Epic Games Launcher by reading its manifests .item
    public class EpicLauncherProvider : ILauncherProvider
    {
        public string SourceName => "Epic Games";

        // Epics mainfest for games
        private const string ManifestDir = @"C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests";

        // Scans the mainfest and returns a list of installed games. Each game is represented by a GameData object
        public Task<List<GameData>> ScanAsync()
        {
            var results = new List<GameData>();

            // If the manifest directory doesn't exist, return an empty list
            if (!Directory.Exists(ManifestDir))
                return Task.FromResult(results);

            // Go through each .item file in the manifest directory and parse it to extract game information
            foreach (var file in Directory.EnumerateFiles(ManifestDir, "*.item"))
            {
                try
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(file));
                    var root = doc.RootElement;

                    string? displayName = root.TryGetProperty("DisplayName", out var dn) ? dn.GetString() : null;
                    string? installLocation = root.TryGetProperty("InstallLocation", out var il) ? il.GetString() : null;

                    if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(installLocation))
                        continue;

                    if (!Directory.Exists(installLocation))
                        continue;

                    // AppName is Epic's internal catalog identifier when present — stable
                    // across rescans. Falls back to a hash of name+path if the field is
                    // missing on a given manifest, so this never breaks even if that
                    // assumption turns out wrong for some manifest variant.
                    string? appName = root.TryGetProperty("AppName", out var an) ? an.GetString() : null;
                    string id = !string.IsNullOrWhiteSpace(appName)
                        ? $"epic_{appName}"
                        : IdGenerator.ComputeStableId(SourceName, displayName!, installLocation!);

                    results.Add(new GameData
                    {
                        Id = id,
                        Name = displayName!,
                        Path = installLocation!,
                        Category = "Uncategorized",
                        IsInstalled = true,
                        SourceLauncher = SourceName
                    });
                }
                catch
                {
                    // one corrupt/partial manifest shouldn't fail the whole scan
                }
            }

            return Task.FromResult(results);
        }
    }
}