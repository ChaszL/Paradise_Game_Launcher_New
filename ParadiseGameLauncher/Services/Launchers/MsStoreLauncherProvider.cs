using ParadiseGameLauncher.Models;
using ParadiseGameLauncher.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ParadiseGameLauncher.Services.Launchers
{
    // Finds games installed via the Xbox app / PC Game Pass by scanning the
    // default "XboxGames" folder on each fixed drive. Known limitation: this
    // won't find games installed to a custom folder name, since Windows no
    // longer enforces that convention. Accepted tradeoff for simplicity —
    // revisit with a PackageManager-based fallback if this proves too limited.
    public class MsStoreLauncherProvider : ILauncherProvider
    {
        public string SourceName => "Microsoft Store";

        public Task<List<GameData>> ScanAsync()
        {
            var results = new List<GameData>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Fixed || !drive.IsReady)
                    continue;

                List<string> xboxFolders;
                try
                {
                    xboxFolders = Directory.EnumerateDirectories(drive.RootDirectory.FullName)
                        .Where(d => Path.GetFileName(d).Contains("xbox", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
                catch
                {
                    continue; // can't read this drive's root, skip it
                }

                foreach (var xboxGamesPath in xboxFolders)
                    foreach (var gameDir in Directory.EnumerateDirectories(xboxGamesPath))
                    {
                        try
                    {
                        var name = Path.GetFileName(gameDir);
                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        results.Add(new GameData
                        {
                            Id = IdGenerator.ComputeStableId(SourceName, name),
                            Name = name,
                            Path = FindExecutable(gameDir) ?? gameDir,
                            Category = "Uncategorized",
                            IsInstalled = true,
                            SourceLauncher = SourceName
                        });
                    }
                    catch
                    {
                        // one bad folder shouldn't fail the whole scan
                    }
                }
            }

            return Task.FromResult(results);
        }

        // Looks for a launchable .exe within a game's folder, capped at a shallow
        // depth so this doesn't turn into a full recursive drive crawl.
        private static string? FindExecutable(string dir, int maxDepth = 3)
        {
            try
            {
                if (!Directory.Exists(dir)) return null;

                var exe = Directory.EnumerateFiles(dir, "*.exe", SearchOption.TopDirectoryOnly)
                                    .FirstOrDefault();
                if (exe != null) return exe;

                if (maxDepth <= 0) return null;

                foreach (var sub in Directory.EnumerateDirectories(dir))
                {
                    var found = FindExecutable(sub, maxDepth - 1);
                    if (found != null) return found;
                }
            }
            catch { }

            return null;
        }
    }
}