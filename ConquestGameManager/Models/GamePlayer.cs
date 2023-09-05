using System;
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
        
        public GamePlayer(CSteamID steamID, string username, DateTime firstJoined, DateTime lastJoined)
        {
            SteamID = steamID;
            Username = username;
            FirstJoined = firstJoined;
            LastJoined = lastJoined;
            Playtime = 0;
        }

        public static void UpdateValue(string coloumnName, int value)
        {
            switch (coloumnName.ToLower())
            {
                default:
                    break;
            }
        }

        public void UpdateDateTime(string coloumnName, DateTime dateTime)
        {
            switch (coloumnName.ToLower())
            {
                case "last joined":
                    LastJoined = dateTime;
                    break;
                default:
                    break;
            }
        }
    }
}
