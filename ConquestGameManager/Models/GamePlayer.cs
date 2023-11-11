using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Core;
using Steamworks;

namespace ConquestGameManager.Models
{
    public class GamePlayer
    {
        public CSteamID SteamID { get; set; }
        public string Username { get; set; }
        public DateTime FirstJoined { get; set; }
        public DateTime LastJoined { get; set; }
        public int Playtime { get; set; }
        public string RankKitsMsg { get; set; }
        public List<Kit> RankKits { get; set; }
        public string CustomKitsMsg { get; set; }
        public List<Kit> CustomKits { get; set; }
        public Dictionary<Kit, DateTime> LastKitClaim { get; set; }
        public Kit LastUsedKit { get; set; }
        public int Rank { get; set; }
        public int Xp { get; set; }

        public GamePlayer(CSteamID steamID, string username, DateTime firstJoined, DateTime lastJoined)
        {
            SteamID = steamID;
            Username = username;
            Rank = 0;
            Xp = 0;
            FirstJoined = firstJoined;
            LastJoined = lastJoined;
            Playtime = 0;

            LastKitClaim = new Dictionary<Kit, DateTime>();
            LastUsedKit = null;

            BuildAllKits();
        }

        public static void UpdateValue(string columnName, int value)
        {
            switch (columnName.ToLower())
            {
                default:
                    break;
            }
        }

        public void UpdateDateTime(string columnName, DateTime dateTime)
        {
            switch (columnName.ToLower())
            {
                case "last joined":
                    LastJoined = dateTime;
                    break;
                default:
                    break;
            }
        }

        public void BuildAllKits()
        {
            BuildRankKits();
            BuildCustomKits();
        }

        private void BuildRankKits()
        {
            var config = Main.Instance.Configuration.Instance;
            var selectedRank = config.Ranks.FirstOrDefault(r => r.RankID == Rank);

            if (selectedRank != null)
            {
                RankKits = selectedRank.RankKits.ToList();
                RankKitsMsg = string.Join(", ", RankKits.Select(kit => kit.KitName));
            }
            else
            {
                RankKits = new List<Kit>();
                RankKitsMsg = string.Empty;
            }
        }

        private void BuildCustomKits()
        {
            var player = new RocketPlayer(SteamID.ToString());
            var perms = R.Permissions.GetPermissions(player).Where(k => k.Name.Contains("kit.")).Select(k => k.Name.Replace("kit.", "")).ToList();
            if (!perms.Any())
            {
                return;
            }

            var customKits = perms.Select(kitName => new Kit(kitName, 0, false, false, false)).ToList();
            CustomKitsMsg = string.Join(", ", perms);
            CustomKits = customKits;
        }

    }
}
