using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParadiseGameLauncher.Models
{
    // object used to formatt JSON data for saving and loading game records
    public class GameRecord
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? ArtPath { get; set; } = null;
        public string Category { get; set; } = "Uncategorized";
        public bool IsInstalled { get; set; } = false;
        public string SourceLauncher { get; set; } = "Custom";

        // object used to convert GameData to GameRecord for saving
        public static GameRecord FromGameData(GameData data)
        {
            return new GameRecord
            {
                Id = data.Id,
                Name = data.Name,
                Path = data.Path,
                ArtPath = data.ArtPath,
                Category = data.Category,
                IsInstalled = data.IsInstalled,
                SourceLauncher = data.SourceLauncher
            };
        }

        // object used to convert GameRecord to GameData for loading
        public GameData ToGameData()
        {
            return new GameData
            {
                Id = Id,
                Name = Name,
                Path = Path,
                ArtPath = ArtPath,
                Category = Category,
                IsInstalled = IsInstalled,
                SourceLauncher = SourceLauncher
            };
        }
    }

}
