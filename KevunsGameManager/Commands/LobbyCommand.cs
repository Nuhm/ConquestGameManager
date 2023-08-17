using KevunsGameManager.Managers;
using Org.BouncyCastle.Crypto.Engines;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KevunsGameManager.Commands
{
    internal class LobbyCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "lobby";

        public string Help => "Command to return to the lobby";

        public string Syntax => "/lobby";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = caller as UnturnedPlayer;

            GameManager.Instance.PlayerLeftGame(player);

            SpawnManager.Instance.RespawnPlayer(player);
            UnturnedChat.Say(caller, "Returned to lobby");
        }
    }
}
