using System;
using System.Collections.Generic;

namespace ConquestGameManager.Models
{
    public class Rank
    {
        public int RankID { get; set; }
        public string RankName { get; set; }
        public int RequiredXp { get; set; }
        public int HealthLevel { get; set; }
        public int MovementLevel { get; set; }
        public int JumpLevel { get; set; }
        public int StaminaLevel { get; set; }
        public List<Kit> RankKits { get; set; }

        public Rank()
        {
            RankKits = new List<Kit>();
        }

        public Rank(int rankID, string rankName, int requiredXp, int healthLevel, int movementLevel, int jumpLevel, int staminaLevel)
        {
            RankID = rankID;
            RankName = rankName;
            RequiredXp = requiredXp;
            HealthLevel = healthLevel;
            MovementLevel = movementLevel;
            JumpLevel = jumpLevel;
            StaminaLevel = staminaLevel;
            RankKits = new List<Kit>();
        }
    }
}