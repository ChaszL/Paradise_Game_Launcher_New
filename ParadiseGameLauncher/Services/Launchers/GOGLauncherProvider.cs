using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using ParadiseGameLauncher.Models;

namespace ParadiseGameLauncher.Services.Launchers
{
    // Finds games installed via GOG Galaxy by reading its registry Games key.
    public class GOGLauncherProvider : ILauncherProvider
    {
        public string SourceName => "GOG Galaxy";

        public Task<List<GameData>> ScanAsync()
        {
            var results = new List<GameData>();

            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\GOG.com\Games")
                          ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GOG.com\Games");

            if (key == null)
                return Task.FromResult(results);

            foreach (var gameId in key.GetSubKeyNames())
            {
                try
                {
                    using var gameKey = key.OpenSubKey(gameId);
                    var path = gameKey?.GetValue("path") as string;
                    var name = gameKey?.GetValue("gameName") as string;

                    // Check the actual value we read (path), not a registry key
                    // GOG doesn't use.
                    if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                        continue;

                    results.Add(new GameData
                    {
                        Id = $"GOG_{gameId}",
                        Name = string.IsNullOrWhiteSpace(name) ? Path.GetFileName(path.TrimEnd('\\', '/')) : name,
                        Path = path,
                        Category = "Uncategorized",
                        IsInstalled = true,
                        SourceLauncher = SourceName
                    });
                }
                catch
                {
                    // one bad registry entry shouldn't fail the whole scan
                }
            }

            return Task.FromResult(results);
        }
    }
}