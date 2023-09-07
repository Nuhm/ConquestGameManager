using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Chat;

namespace ConquestGameManager.Commands
{
    internal class GamemodesCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "gamemodes";

        public string Help => "Command to list all available gamemodes";

        public string Syntax => "/gamemodes";

        public List<string> Aliases => new();

        public List<string> Permissions => new();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var gamemodes = Main.Instance.Configuration.Instance.Gamemodes;
            
            if (gamemodes.Count == 0)
            {
                UnturnedChat.Say(caller, "No gamemodes available.");
                return;
            }
            
            var enabledGamemodeNames = gamemodes
                .Where(gamemode => gamemode.IsEnabled)
                .Select(gamemode => gamemode.Name);

            var gamemodeNames = enabledGamemodeNames.ToList();
            if (!gamemodeNames.Any())
            {
                UnturnedChat.Say(caller, "No enabled gamemodes available.");
                return;
            }

            var gamemodesMessage = "Available gamemodes: " + string.Join(", ", gamemodeNames);
            UnturnedChat.Say(caller, gamemodesMessage);
        }
    }
}