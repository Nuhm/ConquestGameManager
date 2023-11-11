using System;
using System.Linq;
using System.Threading.Tasks;
using ConquestGameManager.Managers;
using Rocket.Unturned.Chat;

namespace ConquestGameManager.Models
{
    public class RankManager
    {
        private readonly Config config;
        private readonly DatabaseManager databaseManager; // Add this field

        public async Task CheckAndHandleRankUp(GamePlayer gamePlayer)
        {
            var currentRankID = gamePlayer.Rank;
            var nextRankID = currentRankID + 1;
            var nextRank = config.Ranks.FirstOrDefault(rank => rank.RankID == nextRankID);

            if (nextRank != null && gamePlayer.XP >= nextRank.RequiredXP)
            {
                await databaseManager.PromotePlayerAsync(gamePlayer.SteamID);
                UnturnedChat.Say(gamePlayer.SteamID, $"Congratulations! You've been promoted to {nextRank.RankName}!");
            }
        }
    }
}