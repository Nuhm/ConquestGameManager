using System;
using System.Collections.Generic;
using Steamworks;

namespace ConquestGameManager.Models
{
    public class Team
    {
        public byte TeamID { get; set; }
        public string TeamName { get; set; }
        public List<CSteamID> TeamMembers { get; } = new List<CSteamID>();

        public Team(byte teamID, string teamName)
        {
            TeamID = teamID;
            TeamName = teamName;
        }

        public void AddMember(CSteamID steamID)
        {
            TeamMembers.Add(steamID);
        }

        public void RemoveMember(CSteamID steamID)
        {
            TeamMembers.Remove(steamID);
        }

        public int GetMemberCount()
        {
            return TeamMembers.Count;
        }
    }
}