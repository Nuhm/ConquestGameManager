using System;
using System.Collections.Generic;

namespace ConquestGameManager.Models
{
    public static class RankManager
    {
        public static List<Rank> Ranks { get; private set; }

        static RankManager()
        {
            Ranks = new List<Rank>();
        }

        public static void AddRank(string rankName)
        {
            Ranks.Add(new Rank(rankName));
        }

        public static Rank GetRank(string rankName)
        {
            return Ranks.Find(rank => rank.RankName.Equals(rankName, StringComparison.OrdinalIgnoreCase));
        }
    }
}