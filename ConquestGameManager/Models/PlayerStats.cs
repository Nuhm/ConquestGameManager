using System;
using Steamworks;

namespace ConquestGameManager.Models
{
    public class PlayerStats
    {
        public CSteamID SteamID { get; set; }
        public string Username { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public double KDR { get; set; }
        public int Headshots { get; set; }
        public double HeadshotAccuracy { get; set; }
        
        public PlayerStats(CSteamID steamID, string username, int kills, int deaths, double kdr, int headshots, double headshotAccuracy)
        {
            SteamID = steamID;
            Username = username;
            Kills = kills;
            Deaths = deaths;
            KDR = kdr;
            Headshots = headshots;
            HeadshotAccuracy = headshotAccuracy;
        }

        public void UpdateValue(string coloumnName, int value)
        {
            switch (coloumnName.ToLower())
            {
                case "kills":
                    Kills = value;
                    break;
                case "deaths":
                    Deaths = value;
                    break;
                case "headshots":
                    Headshots = value;
                    break;
                default:
                    break;
            }
        }
    }
}