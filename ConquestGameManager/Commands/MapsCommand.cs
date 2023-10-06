using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Chat;

namespace ConquestGameManager.Commands
{
    internal class MapsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "maps";
        public string Help => "Command to list all available maps";
        public string Syntax => "/maps";
        public List<string> Aliases => new();
        public List<string> Permissions => new() { "cq.player" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var maps = Main.Instance.Configuration.Instance.Maps;

            if (maps.Count == 0)
            {
                UnturnedChat.Say(caller, "No maps available.");
                return;
            }

            var enabledMapNames = maps
                .Where(map => map.IsEnabled)
                .Select(map => map.MapName);

            var mapNames = enabledMapNames.ToList();
            if (!mapNames.Any())
            {
                UnturnedChat.Say(caller, "No enabled maps available.");
                return;
            }

            var mapsMessage = "Available enabled maps: " + string.Join(", ", mapNames);
            UnturnedChat.Say(caller, mapsMessage);
        }
    }
}