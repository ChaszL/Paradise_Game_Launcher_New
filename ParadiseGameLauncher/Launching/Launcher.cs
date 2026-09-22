using System;
using System.Diagnostics;
using ParadiseGameLauncher.Models;

namespace ParadiseGameLauncher.Launching
{
    // constructor for the result of launching a game
    public class LaunchResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // static class responsible for launching games
    public static class Launcher
    {
        public static LaunchResult Launch(GameData game)
        {
            // Check if the game path is valid if not return false and an error message
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = game.Path,
                    UseShellExecute = true
                };

                Process.Start(startInfo);

                return new LaunchResult { Success = true };
            }
            catch (Exception ex)
            {
                return new LaunchResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}