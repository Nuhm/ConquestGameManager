using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KevunsGameManager.Models
{
    public class Map
    {
        public int MapID { get; set; }
        public string MapName { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }

        public List<Location> Locations { get; set; }

        public Map(int mapID, string mapName, int minPlayers, int maxPlayers, List<Location> locations) 
        {
            MapID = mapID;
            MapName = mapName;
            MinPlayers = minPlayers;
            MaxPlayers = maxPlayers;
            Locations = locations;
        }

        public Map() {
        
        }
    }
}
