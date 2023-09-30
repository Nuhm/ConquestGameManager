using System;
using System.Collections.Generic;

namespace ConquestGameManager.Models
{
    public class Rank
    {
        public string RankName { get; set; }
        public List<string> RankKits { get; set; }

        // Parameterless constructor for serialization
        public Rank()
        {
            RankKits = new List<string>();
        }

        public Rank(string rankName)
        {
            RankName = rankName;
            RankKits = new List<string>();
        }
    }
}