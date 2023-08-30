using System.Collections.Generic;

namespace KevunsGameManager.Models
{
    public class Map
    {
        public int MapID { get; set; }
        public string MapName { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }

        public List<Time> TimeWeights { get; set; }
        public List<Location> Locations { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public Map(int mapID, string mapName, int minPlayers, int maxPlayers, List<Time> timeWeights, List<Location> locations, bool isEnabled) 
        {
            MapID = mapID;
            MapName = mapName;
            MinPlayers = minPlayers;
            MaxPlayers = maxPlayers;
            TimeWeights = timeWeights;
            Locations = locations;
            IsEnabled = isEnabled;
        }

        public Map() {
        
        }
    }
}
