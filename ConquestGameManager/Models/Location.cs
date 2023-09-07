using System;
using System.Xml.Serialization;

// Import the XmlSerializer namespace

namespace ConquestGameManager.Models
{
    public class Location
    {
        public int LocationID { get; set; }
        public Boolean HasCooldown { get; set; }

        public float LocationX { get; set; }
        public float LocationY { get; set; }
        public float LocationZ { get; set; }

        [XmlIgnore] // Prevent LastUsed from being automatically serialized/deserialized
        public DateTime LastUsed { get; set; }

        public Location(int locationID, bool hasCooldown, float locationX, float locationY, float locationZ)
        {
            LocationID = locationID;
            HasCooldown = hasCooldown;

            LocationX = locationX;
            LocationY = locationY;
            LocationZ = locationZ;
        }

        public Location() { }
    }
}
