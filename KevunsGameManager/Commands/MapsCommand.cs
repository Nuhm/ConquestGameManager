using Rocket.API;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Player;
using UnityEngine;
using KevunsGameManager.Models;

namespace KevunsGameManager.Commands
{
    internal class MapsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "maps";

        public string Help => "Command to list all available maps";

        public string Syntax => "/maps";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var maps = Main.Instance.Configuration.Instance.Maps;

            if (maps.Count == 0)
            {
                UnturnedChat.Say(caller, "No maps available.");
                return;
            }

            var mapNames = maps.Select(map => map.MapName);
            var mapsMessage = "Available maps: " + string.Join(", ", mapNames);
            UnturnedChat.Say(caller, mapsMessage);
        }
    }
}