using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KevunsGameManager.Models
{
    public class Location
    {
        public int LocationID { get; set; }
        public Boolean HasCooldown { get; set; }

        public float LocationX { get; set; }
        public float LocationY { get; set; }
        public float LocationZ { get; set; }

        public Location(int locationID, Boolean hasCooldown, float locationX, float locationY, float locationZ)
        {
            LocationID = locationID;
            HasCooldown = hasCooldown;

            LocationX = locationX;
            LocationY = locationY;
            LocationZ = locationZ;
        }

        public Location() {

        }
    }
}
