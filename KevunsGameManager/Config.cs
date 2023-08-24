using KevunsGameManager.Models;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Steam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace KevunsGameManager
{
    public class Config : IRocketPluginConfiguration
    {
        public bool LoggingEnabled { get; set; }
        public string ConnectionString { get; set; }

        public int CooldownDurationSeconds { get; set; }

        public float LobbyX { get; set; }
        public float LobbyY { get; set; }
        public float LobbyZ { get; set; }

        public List<Map> Maps { get; set; }
        public List<Gamemode> Gamemodes { get; set; }
        
        public int DeployLimitSeconds { get; set; }
        public int RespawnLimitSeconds { get; set; }

        public void LoadDefaults()
        {
            ConnectionString = "server=localhost;user=root;database=unturned;port=3306;password=root";
            CooldownDurationSeconds = 5;

            LobbyX = 0;
            LobbyY = 0;
            LobbyZ = 0;

            Maps = new List<Map>
            {
                new Map(1, "Test", 0, 40, new List<Location>
                {
                    new Location(1, false, 0, 0, 0),
                    new Location(2, false, 0, 0, 0),
                    new Location(3, false, 0, 0, 0)
                }, true),
                new Map(2, "Warehouse", 0, 40, new List<Location>
                {
                    new Location(1, false, 0, 0, 0),
                    new Location(2, false, 0, 0, 0),
                    new Location(3, false, 0, 0, 0)
                }, true),
                new Map(3, "Bunker", 0, 40, new List<Location>
                {
                    new Location(1, false, 0, 0, 0),
                    new Location(2, false, 0, 0, 0),
                    new Location(3, false, 0, 0, 0)
                }, true)
            };
            Gamemodes = new List<Gamemode>
            {
                new Gamemode
                {
                    Name = "FFA",
                    Duration = 20,
                    HasTeams = false,
                    IsEnabled = true,
                },
                new Gamemode
                {
                    Name = "TDM",
                    Duration = 20,
                    HasTeams = true,
                    IsEnabled = true
                }
            };
            DeployLimitSeconds = 10;
            RespawnLimitSeconds = 10;
        }
    }
}
