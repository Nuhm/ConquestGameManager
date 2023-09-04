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
        public static TeamManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TeamManager();
                }
                return instance;
            }
        }

        public void GroupPlayer(UnturnedPlayer player)
        {
            string currentGameModeName = GameManager.Instance.gameModes[GameManager.Instance.currentGameModeIndex].Name;

            if (currentGameModeName.Equals("ffa", StringComparison.OrdinalIgnoreCase))
            {
                AssignPlayerToGroup(player, (CSteamID)0, "No Team");
            }
            
            if (currentGameModeName.Equals("tdm", StringComparison.OrdinalIgnoreCase))
            {
                Random random = new Random();
                int randomTeamID = random.Next(2, 4);

                string groupName = randomTeamID == 2 ? "BLUFOR" : "OPFOR";
                CSteamID steamGroupId = randomTeamID == 2 ? (CSteamID)2 : (CSteamID)3;

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