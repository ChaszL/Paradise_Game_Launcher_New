using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using ParadiseGameLauncher.Models;

namespace ParadiseGameLauncher.Services.Launchers
{
    // Finds games installed via Ubisoft Connect by reading its registry Installs
    public class UbisoftLauncherProvider : ILauncherProvider
    {
        public string SourceName => "Ubisoft Connect";

        // Scans the registry for installed games and returns a list of GameData objects
        public Task<List<GameData>> ScanAsync()
        {
            var results = new List<GameData>();

            // Ubisoft Connect stores its installed games in the registry under the following keys:
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Ubisoft\Launcher\Installs")
                          ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Ubisoft\Launcher\Installs");

            // If the key doesn't exist, return an empty list
            if (key == null)
                return Task.FromResult(results);

            // Iterate through each subkey (game) and extract the install directory
            foreach (var gameId in key.GetSubKeyNames())
            {
                try
                {
                    using var gameKey = key.OpenSubKey(gameId);
                    var installDir = gameKey?.GetValue("InstallDir") as string;

                    if (string.IsNullOrWhiteSpace(installDir) || !Directory.Exists(installDir))
                        continue;

                    var name = Path.GetFileName(installDir.TrimEnd('\\', '/'));

                    results.Add(new GameData
                    {
                        // The registry subkey name is Ubisoft's own stable identifier
                        // for this install — no hashing needed, same role as Steam's AppID.
                        Id = $"ubisoft_{gameId}",
                        Name = string.IsNullOrWhiteSpace(name) ? installDir : name,
                        Path = installDir,
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