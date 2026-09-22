using CommunityToolkit.Mvvm.ComponentModel;
using ParadiseGameLauncher.Models;
using ParadiseGameLauncher.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ParadiseGameLauncher.Models
{
    public partial class GameData : ObservableObject
    {
        // Creates a unique identifier for each game found by the launcher
        [ObservableProperty]
        private string _id = string.Empty;

        // Creates a property for the name of the game
        [ObservableProperty]
        private string _name = string.Empty;

        // Creates a property for the path of the game
        [ObservableProperty]
        private string _path = string.Empty;

        // Creates a property for the path of the game art
        [ObservableProperty]
        private string? _artPath;

        // Creates a property for the category of the game
        // Default value is "Uncategorized"
        [ObservableProperty]
        private string _category = "Uncategorized";

        // Creates a property for the installation status of the game
        [ObservableProperty]
        private bool _isInstalled;

        // Creates a property for the launcher of the game
        [ObservableProperty]
        private string _sourceLauncher = "Custom";
    }

    public static class GameDataCreator
    {
        // Creates a GameData object from a given file path of a game
        public static GameData CreateFromPath(string filepath)
        {
            string name = Path.GetFileNameWithoutExtension(filepath);

            return new GameData
            {
                Id = IdGenerator.ComputeStableId("Custom", name, filepath),
                Name = name,
                Path = filepath,
                Category = "Uncategorized",
                IsInstalled = false,
                SourceLauncher = "Custom"
            };
        }
    }
}