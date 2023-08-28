﻿using System.Collections.Generic;
using KevunsGameManager.Managers;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;

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

            if (!GameManager.Instance.ActivePlayers.Contains(player))
            {
                UnturnedChat.Say(caller, "You are already in the lobby!");
                return;
            }
            
            GameManager.Instance.PlayerLeftGame(player);

            SpawnManager.Instance.RespawnPlayer(player);
            UnturnedChat.Say(caller, "Returned to lobby");
        }
    }
}
