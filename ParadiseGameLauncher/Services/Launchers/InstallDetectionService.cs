using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ParadiseGameLauncher.Models;

namespace ParadiseGameLauncher.Services
{
    // Determines install status for Custom shortcuts 
    public static class InstallDetectionService
    {
        // Checks whether a custom shortcut points to an installed game, using the shortcut's own resolved target, then a fuzzy-name
        // match against games the launcher providers already found.
        public static bool CheckInstalled(GameData shortcutGame, IEnumerable<GameData> knownInstalledGames)
        {
            try
            {
                var target = ResolveShortcutTarget(shortcutGame.Path);
                if (string.IsNullOrWhiteSpace(target))
                    return false;

                // checks if the target shortcut points to a real game or not a steam game
                if (!target.StartsWith("steam://", StringComparison.OrdinalIgnoreCase) &&
                    (File.Exists(target) || Directory.Exists(target)))
                {
                    return true;
                }

                // steam:// targets and anything else fall through to matching against what the providers already discovered
                return knownInstalledGames.Any(known => IsNameSimilar(known.Name, shortcutGame.Name));
            }
            catch
            {
                return false;
            }
        }

        // Resolves a .lnk or .url shortcut file to its real target path or URI
        private static string? ResolveShortcutTarget(string shortcutPath)
        {
            try
            {
                var ext = Path.GetExtension(shortcutPath) ?? string.Empty;

                if (string.Equals(ext, ".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                    if (shellType == null) return null;

                    dynamic? shell = Activator.CreateInstance(shellType);
                    if (shell == null) return null;

                    dynamic lnk = shell.CreateShortcut(shortcutPath);
                    string? target = lnk?.TargetPath as string;
                    string? args = lnk?.Arguments as string;

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(args))
                        {
                            var m = Regex.Match(args, @"-applaunch\s+(\d+)", RegexOptions.IgnoreCase);
                            if (m.Success)
                            {
                                var id = m.Groups[1].Value;
                                try { Marshal.ReleaseComObject(lnk); } catch { }
                                try { Marshal.ReleaseComObject(shell); } catch { }
                                return $"steam://run/{id}";
                            }
                        }
                    }
                    catch { }

                    try { Marshal.ReleaseComObject(lnk); } catch { }
                    try { Marshal.ReleaseComObject(shell); } catch { }
                    return target;
                }

                if (string.Equals(ext, ".url", StringComparison.OrdinalIgnoreCase))
                {
                    var lines = File.ReadAllLines(shortcutPath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("URL=", StringComparison.OrdinalIgnoreCase))
                            return line.Substring(4).Trim();

                        if (line.StartsWith("IconFile=", StringComparison.OrdinalIgnoreCase))
                        {
                            var icon = line.Substring(9).Trim();
                            if (!string.IsNullOrWhiteSpace(icon)) return icon;
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        // Fuzzy name match, ported as-is from the old code. Normalizes both
        // names (lowercase, letters/digits only) and requires most tokens
        // from the game name to appear in the candidate name.
        private static bool IsNameSimilar(string candidateName, string gameName)
        {
            if (string.IsNullOrWhiteSpace(candidateName) || string.IsNullOrWhiteSpace(gameName))
                return false;

            var f = NormalizeName(candidateName);
            var g = NormalizeName(gameName);
            if (string.IsNullOrEmpty(f) || string.IsNullOrEmpty(g))
                return false;

            if (f.Contains(g) || g.Contains(f))
                return true;

            var tokens = Regex.Split(g, "[^a-z0-9]+")
                               .Where(t => !string.IsNullOrWhiteSpace(t))
                               .ToArray();
            if (tokens.Length == 0)
                return false;

            int matches = tokens.Count(t => f.Contains(t));
            return matches >= Math.Max(1, tokens.Length - 1);
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            var chars = name.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray();
            return new string(chars);
        }
    }
}