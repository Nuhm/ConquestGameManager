using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;

namespace ConquestGameManager.Commands
{
    internal class LeaderboardCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "leaderboard";
        public string Help => "Command to list the top players";
        public string Syntax => "/leaderboard [page]";
        public List<string> Aliases => new();
        public List<string> Permissions => new() { "cq.player" };

        public async void Execute(IRocketPlayer caller, string[] command)
        {
            var player = caller as UnturnedPlayer;
            var page = 1;

            if (command.Length > 0)
            {
                if (!int.TryParse(command[0], out page) || page < 1)
                {
                    UnturnedChat.Say(player, "Invalid page number. Usage: /leaderboard [page]");
                    return;
                }
            }

            const int playersPerPage = 4;
            var startIndex = (page - 1) * playersPerPage;

            var topPlayers = await Main.Instance.DatabaseManager.GetTopPlayersByKillsAsync(playersPerPage, startIndex);

            if (topPlayers is { Count: > 0 })
            {
                UnturnedChat.Say(player, $"--- Leaderboard - Page {page} ---");

                for (var i = 0; i < topPlayers.Count; i++)
                {
                    var playerStats = topPlayers[i];
                    var rank = startIndex + i + 1;

                    UnturnedChat.Say(player, $"{rank}. {playerStats.Username} - Kills: {playerStats.Kills}");
                }
            }
            else
            {
                UnturnedChat.Say(player, "No players found on the leaderboard.");
            }
        }
    }
}
