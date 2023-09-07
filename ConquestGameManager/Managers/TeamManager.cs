using System;
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

        public void GroupPlayer(UnturnedPlayer player)
        {
            var currentGameModeName = GameManager.Instance.gameModes[GameManager.Instance.currentGameModeIndex].Name;

            if (currentGameModeName.Equals("ffa", StringComparison.OrdinalIgnoreCase))
            {
                AssignPlayerToGroup(player, (CSteamID)0, "No Team");
            }
            
            if (currentGameModeName.Equals("tdm", StringComparison.OrdinalIgnoreCase))
            {
                var random = new Random();
                var randomTeamID = random.Next(2, 4);

                var groupName = randomTeamID == 2 ? "BLUFOR" : "OPFOR";
                var steamGroupId = randomTeamID == 2 ? (CSteamID)2 : (CSteamID)3;

                AssignPlayerToGroup(player, steamGroupId, groupName);
            }

            Logger.Log($"Assigned {player} to group");
        }

        private void AssignPlayerToGroup(UnturnedPlayer player, CSteamID steamGroupId, string groupName)
        {
            var group = GroupManager.getOrAddGroup(steamGroupId, groupName, out var wasCreated);

            if (!wasCreated)
            {
                group.name = groupName;
                GroupManager.sendGroupInfo(group);
            }

            player.Player.quests.ServerAssignToGroup(steamGroupId, EPlayerGroupRank.MEMBER, true);
        }
    }
}