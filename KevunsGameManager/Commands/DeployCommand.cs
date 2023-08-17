using KevunsGameManager.Managers;
using KevunsGameManager.Models;
using Rocket.API;
using Rocket.Core.Steam;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KevunsGameManager.Commands
{
    internal class DeployCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "deploy";

        public string Help => "Command to deploy to game";

        public string Syntax => "/deploy";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = caller as UnturnedPlayer;

            GameManager.Instance.PlayerJoinedGame(player);

            SpawnManager.Instance.RespawnPlayer(player);
            UnturnedChat.Say(caller, "Deployed to game");
        }
    }
}