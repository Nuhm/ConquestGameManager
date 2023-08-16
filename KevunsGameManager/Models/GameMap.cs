using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KevunsGameManager.Models
{
    public class GameMap
    {
        public int MapID { get; set; }
        public int LocationID { get; set; }
        public DateTime LastUsed { get; set; }


        public GameMap(int mapID, int locationID, DateTime lastUsed) 
        {
            MapID = mapID;
            LocationID = locationID;
            LastUsed = lastUsed;
        }

        public void UpdateValue(string columnName, int id)
        {
            switch (columnName.ToLower())
            {
                default:
                    break;
            }
        }

        public void UpdateLastUsedTime(string coloumnName, DateTime dateTime)
        {
            switch (coloumnName.ToLower())
            {
                case "last joined":
                    LastUsed = dateTime;
                    break;
                default:
                    break;
            }
        }
    }
}
