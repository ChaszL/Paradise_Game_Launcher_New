using System;
using System.Security.Cryptography;
using System.Text;

namespace ParadiseGameLauncher.Utilities
{
    // Deterministic Id generation for sources with no natural launcher-provided
    // identifier (e.g. custom shortcuts, XboxGames folder scans). Same source +
    // name (+ optional path) always produces the same Id across rescans, which
    // reconciliation depends on.
    public static class IdGenerator
    {
        public static string ComputeStableId(string source, string name, string? path = null)
        {
            string normalized = path == null
                ? $"{source.ToLowerInvariant()}|{name.ToLowerInvariant().Trim()}"
                : $"{source.ToLowerInvariant()}|{name.ToLowerInvariant().Trim()}|{path.ToLowerInvariant().Trim()}";

            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            return Convert.ToHexString(bytes)[..16];
        }
    }
}