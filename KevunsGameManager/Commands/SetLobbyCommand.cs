using Rocket.API;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using Rocket.Unturned.Player;
using UnityEngine;
using KevunsGameManager.Models;

namespace KevunsGameManager.Commands
{
    internal class SetLobbyCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "setlobby";

        public string Help => "Command to set server lobby spawn point";

        public string Syntax => "/setlobby";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = caller as UnturnedPlayer;
            
            Main.Instance.Configuration.Instance.LobbyX = player.Position.x;
            Main.Instance.Configuration.Instance.LobbyY = player.Position.y;
            Main.Instance.Configuration.Instance.LobbyZ = player.Position.z;

            Main.Instance.Configuration.Save();

            Utility.Say(caller, "Server lobby spawn point updated.");
        }
    }
}
