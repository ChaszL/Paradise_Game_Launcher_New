using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParadiseGameLauncher.Models;

// used to make all lauchers implement a common interface for launching games
namespace ParadiseGameLauncher.Services.Launchers
{
    // for every launcher they will find games and return those games as gamedata objects
    public interface ILauncherProvider
    {
        // source name of the launchers
        string SourceName { get; }

        // scans for installed games from the launchers and created gamedata objects. 
        // empty list is fine, but expection is not.
        Task<List<GameData>> ScanAsync();
    }
}
