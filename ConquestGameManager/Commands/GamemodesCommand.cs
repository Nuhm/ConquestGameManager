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

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

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

            if (enabledGamemodeNames.Count() == 0)
            {
                UnturnedChat.Say(caller, "No enabled gamemodes available.");
                return;
            }

            var gamemodesMessage = "Available gamemodes: " + string.Join(", ", enabledGamemodeNames);
            UnturnedChat.Say(caller, gamemodesMessage);
        }
    }
}