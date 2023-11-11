using System;
using System.Collections.Generic;

namespace ConquestGameManager.Models
{
    public class Rank
    {
        public int RankID { get; set; }
        public string RankName { get; set; }
        public int RequiredXp { get; set; }
        public List<Kit> RankKits { get; set; }

        public Rank()
        {
            RankKits = new List<Kit>();
        }

        public Rank(int rankID, string rankName, int requiredXp)
        {
            RankID = rankID;
            RankName = rankName;
            RequiredXp = requiredXp;
            RankKits = new List<Kit>();
        }
    }
}