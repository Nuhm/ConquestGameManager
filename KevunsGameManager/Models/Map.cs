using System.Collections.Generic;

namespace KevunsGameManager.Models
{
    public class Map
    {
        public int MapID { get; set; }
        public string MapName { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }

        public List<Location> Locations { get; set; }
        
        public bool IsEnabled { get; set; }

        public Map(int mapID, string mapName, int minPlayers, int maxPlayers, List<Location> locations, bool isEnabled) 
        {
            MapID = mapID;
            MapName = mapName;
            MinPlayers = minPlayers;
            MaxPlayers = maxPlayers;
            Locations = locations;
            IsEnabled = isEnabled;
        }

        public Map() {
        
        }
    }
}
