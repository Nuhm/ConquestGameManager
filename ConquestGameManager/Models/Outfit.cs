using System.Collections.Generic;

namespace ConquestGameManager.Models
{
    public class Outfit
    {
        public int ID { get; set; }
        public ushort Backpack { get; set; }
        public ushort Glasses { get; set; }
        public ushort Hat { get; set; }
        public ushort Mask { get; set; }
        public ushort Pants { get; set; }
        public ushort Shirt { get; set; }
        public ushort Vest { get; set; }

        public Outfit(int id, ushort backpack, ushort glasses, ushort hat, ushort mask, ushort pants, ushort shirt, ushort vest)
        {
            ID = id;
            Backpack = backpack;
            Glasses = glasses;
            Hat = hat;
            Mask = mask;
            Pants = pants;
            Shirt = shirt;
            Vest = vest;
        }
    }
}