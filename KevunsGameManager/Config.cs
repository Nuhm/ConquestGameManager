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

        public float lobbyX { get; set; }
        public float lobbyY { get; set; }
        public float lobbyZ { get; set; }

        public List<Map> Maps { get; set; }
        public List<Gamemode> Gamemodes { get; set; }

        public void LoadDefaults()
        {
            ConnectionString = "server=localhost;user=root;database=unturned;port=3306;password=root";
            CooldownDurationSeconds = 5;

            lobbyX = 0;
            lobbyY = 0;
            lobbyZ = 0;

            Maps = new List<Map>
            {
                new Map(1, "Test", 0, 40, new List<Location>
                {
                    new Location(1, false, 0, 0, 0),
                    new Location(2, false, 0, 0, 0),
                    new Location(3, false, 0, 0, 0)
                }),
                new Map(2, "Warehouse", 0, 40, new List<Location>
                {
                    new Location(1, false, 0, 0, 0),
                    new Location(2, false, 0, 0, 0),
                    new Location(3, false, 0, 0, 0)
                }),
                new Map(3, "Bunker", 0, 40, new List<Location>
                {
                    new Location(1, false, 0, 0, 0),
                    new Location(2, false, 0, 0, 0),
                    new Location(3, false, 0, 0, 0)
                })
            };
            Gamemodes = new List<Gamemode>
            {
                new Gamemode
                {
                    Name = "FFA",
                    HasTeams = false
                },
                new Gamemode
                {
                    Name = "TDM",
                    HasTeams = true
                }
                // Add more gamemode settings as needed
            };
        }
    }
}
