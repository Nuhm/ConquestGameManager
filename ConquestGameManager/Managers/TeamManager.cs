using System;
using System.Collections.Generic;
using System.Linq;
using ConquestGameManager.Models;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace ConquestGameManager.Managers
{
    public class TeamManager
    {
        private static TeamManager instance;
        public static TeamManager Instance => instance ??= new TeamManager();

        private List<Team> teams = new List<Team>();

        private TeamManager()
        {
            InitializeTeams();
        }

        private void InitializeTeams()
        {
            teams.Add(new Team(0, "No Team"));
            teams.Add(new Team(1, "Inactive Players Group"));
            teams.Add(new Team(2, "BLUFOR"));
            teams.Add(new Team(3, "OPFOR"));
        }
        
        public void GroupPlayer(UnturnedPlayer player)
        {
            var currentGameModeName = GameManager.Instance.gameModes[GameManager.Instance.currentGameModeIndex].Name;

            if (currentGameModeName.Equals("ffa", StringComparison.OrdinalIgnoreCase))
            {
                var team = teams.Find(t => t.TeamID == 0);
                AssignPlayerToGroup(player, team);
            }

            if (currentGameModeName.Equals("tdm", StringComparison.OrdinalIgnoreCase))
            {
                // Check if the player is already assigned to a team
                if (!GameManager.Instance.IsPlayerAssignedToTeam(player))
                {
                    // Add player to a random team
                    var random = new Random();
                    var randomTeamID = random.Next(2, 4);
                    var team = teams.Find(t => t.TeamID == randomTeamID);

                    AssignPlayerToGroup(player, team);
                    GameManager.Instance.SetPlayerTeam(player, team.TeamName);
                }
                else
                {
                    // Player is already assigned to a team, add them to that team again
                    var assignedTeam = GameManager.Instance.GetPlayerTeam(player);
                    var team = teams.Find(t => t.TeamName == assignedTeam);

                    AssignPlayerToGroup(player, team);
                }
            }

            Logger.Log($"Assigned {player} to group");
        }

        public void AssignPlayerToGroup(UnturnedPlayer player, Team team)
        {
            var group = GroupManager.getOrAddGroup((CSteamID)team.TeamID, team.TeamName, out var wasCreated);

            if (!wasCreated)
            {
                group.name = team.TeamName;
                GroupManager.sendGroupInfo(group);
            }
            
            player.Player.quests.ServerAssignToGroup((CSteamID)team.TeamID, EPlayerGroupRank.MEMBER, true);
            team.AddMember(player.CSteamID);
        }
    }
}