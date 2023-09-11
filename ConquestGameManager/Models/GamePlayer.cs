using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Logging;
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
        public string KitsMsg { get; set; }
        public List<string> Kits { get; set; }
        public Dictionary<Kit, DateTime> LastKitClaim { get; set; }
        public Kit LastUsedKit { get; set; }

        public GamePlayer(CSteamID steamID, string username, DateTime firstJoined, DateTime lastJoined)
        {
            SteamID = steamID;
            Username = username;
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

        private void BuildAllKits()
        {
            BuildKits();
        }

        private void BuildKits()
        {
            var player = new RocketPlayer(SteamID.ToString());
            var perms = R.Permissions.GetPermissions(player).Where(k => k.Name.Contains("kit.")).Select(k => k.Name.Replace("kit.", "")).ToList() ?? throw new ArgumentNullException("R.Permissions.GetPermissions(player).Where(k => k.Name.Contains(\"kit.\")).Select(k => k.Name.Replace(\"kit.\", \"\")).ToList()");
            if (!perms.Any())
            {
                return;
            }

            var customKitsMsg = perms.Aggregate("", (current, perm) => current + $"{perm}, ");

            if (customKitsMsg.Length != 0)
            {
                customKitsMsg = customKitsMsg.Substring(0, customKitsMsg.Length - 2);
            }

            KitsMsg = customKitsMsg;
            Kits = perms;
        }
    }
}
