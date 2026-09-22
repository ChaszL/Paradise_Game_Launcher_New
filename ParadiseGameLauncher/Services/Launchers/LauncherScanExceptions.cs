using System;

namespace ParadiseGameLauncher.Services.Launchers
{
    // Signals that a provider's scan failed outright (registry unreadable,
    // permissions denied, etc.) — as opposed to a normal empty result, which
    // means the source legitimately has zero games. DiscoveryOrchestrator uses
    // this distinction to avoid treating a failed scan as "everything from
    // this source got uninstalled."
    public class LauncherScanException : Exception
    {
        public string SourceName { get; }

        public LauncherScanException(string sourceName, string message, Exception? inner = null)
            : base(message, inner)
        {
            SourceName = sourceName;
        }
    }
}