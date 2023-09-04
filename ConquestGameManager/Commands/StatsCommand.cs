using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;
using System.Threading.Tasks;
using Steamworks;
using Rocket.Unturned.Chat;
using SDG.Unturned;

namespace ConquestGameManager.Commands
{
    internal class StatsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "stats";
        public string Help => "Command to list player stats";
        public string Syntax => "/stats [playername or SteamID]";
        public List<string> Aliases => new();
        public List<string> Permissions => new();

        public async void Execute(IRocketPlayer caller, string[] command)
        {
            var player = caller as UnturnedPlayer;

            switch (command.Length)
            {
                case 0:
                    if (player != null) await ListPlayerStatsAsync(player, player.CSteamID);
                    break;
                case 1:
                {
                    var targetName = command[0];
                    var targetPlayer = UnturnedPlayer.FromName(targetName);

                    if (targetPlayer != null)
                    {
                        var targetSteamID = targetPlayer.CSteamID;
                        await ListPlayerStatsAsync(player, targetSteamID);
                    }
                    else
                    {
                        UnturnedChat.Say(player, "Player not found. Please enter a valid in-game player name.");
                    }
                    break;
                }
                default:
                    UnturnedChat.Say(player, "Invalid syntax. Usage: /stats [playername or SteamID]");
                    break;
            }
        }

        private static async Task ListPlayerStatsAsync(IRocketPlayer caller, CSteamID targetSteamID)
        {
            var stats = await Main.Instance.DatabaseManager.GetPlayerStatsAsync(targetSteamID);
            var steamName = UnturnedPlayer.FromCSteamID(targetSteamID);

            if (stats != null)
            {
                UnturnedChat.Say(caller, $"Stats for {steamName}:");
                UnturnedChat.Say(caller, $"Kills: {stats.Kills}");
                UnturnedChat.Say(caller, $"Deaths: {stats.Deaths}");
                UnturnedChat.Say(caller, $"KDR: {stats.KDR:F2}");
                UnturnedChat.Say(caller, $"Headshots: {stats.Headshots}");
                UnturnedChat.Say(caller, $"Headshot Accuracy: {stats.HeadshotAccuracy:P2}");
            }
            else
            {
                UnturnedChat.Say(caller, $"Stats for {steamName} not found.");
            }
        }
    }
}
