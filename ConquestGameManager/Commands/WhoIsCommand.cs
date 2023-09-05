using System.Collections.Generic;
using ConquestGameManager.Managers;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;

namespace ConquestGameManager.Commands
{
    public class WhoisCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "whois";
        public string Help => "Displays player information.";
        public string Syntax => "/whois <playername>";
        public List<string> Aliases => new();
        public List<string> Permissions => new();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                UnturnedChat.Say(caller, "Invalid syntax. Usage: /whois <playername>");
                return;
            }

            var playerName = command[0];
            var player = UnturnedPlayer.FromName(playerName);

            if (player != null)
            {
                var steamID = player.CSteamID;
                var databaseManager = Main.Instance.DatabaseManager;
                var previousPlaytime = DatabaseManager.GetPlaytimeAsync(steamID).Result;
                var playerInfo = databaseManager.GetPlayerInfoAsync(steamID).Result;
                var currentPlaytime = databaseManager.GetCurrentPlaytime(steamID);

                if (playerInfo != null)
                {
                    var totalPlaytime = previousPlaytime + currentPlaytime;

                    if (totalPlaytime < 0)
                    {
                        totalPlaytime = 0;
                    }

                    var formattedPlaytime = FormatSeconds(totalPlaytime);
                    var message = $"Player Information for {player.DisplayName}:\n" +
                                  $"SteamID: {playerInfo.SteamID}\n" +
                                  $"Username: {playerInfo.Username}\n" +
                                  $"Total Playtime: {formattedPlaytime}\n" +
                                  $"First Join Date: {playerInfo.FirstJoined}\n";
                    UnturnedChat.Say(caller, message);
                }
                else
                {
                    UnturnedChat.Say(caller, "Player information not found.");
                }
            }
            else
            {
                UnturnedChat.Say(caller, "Player not found.");
            }
        }


        private static string FormatSeconds(int totalSeconds)
        {
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var seconds = totalSeconds % 60;

            var formattedTime = $"{hours}h {minutes}m {seconds}s";
    
            return formattedTime;
        }
    }
}
