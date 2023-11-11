using System.Linq;
using System.Threading.Tasks;
using ConquestGameManager.Models;
using Rocket.Unturned.Chat;

namespace ConquestGameManager.Managers
{
    public class RankManager
    {
        private readonly DatabaseManager databaseManager;

        public RankManager(DatabaseManager databaseManager)
        {
            this.databaseManager = databaseManager;
        }

        public async Task CheckAndHandleRankUp(GamePlayer gamePlayer)
        {
            var currentRankID = gamePlayer.Rank;
            var nextRankID = currentRankID + 1;
            var nextRank = Main.Instance.Configuration.Instance.Ranks.FirstOrDefault(rank => rank.RankID == nextRankID);

            if (nextRank != null && gamePlayer.XP >= nextRank.RequiredXP)
            {
                await databaseManager.PromotePlayerAsync(gamePlayer.SteamID);
                UnturnedChat.Say(gamePlayer.SteamID, $"Congratulations! You've been promoted to {nextRank.RankName}!");
            }
        }
    }
}