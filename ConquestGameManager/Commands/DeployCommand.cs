﻿using System.Collections.Generic;
using ConquestGameManager.Managers;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;

namespace ConquestGameManager.Commands
{
    internal class DeployCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "deploy";
        public string Help => "Command to deploy to game";
        public string Syntax => "/deploy";
        public List<string> Aliases => new();
        public List<string> Permissions => new() { "cq.player" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = caller as UnturnedPlayer;
            
            if (GameManager.ActivePlayers.Contains(player))
            {
                UnturnedChat.Say(caller, "You are already deployed!");
                return;
            }
            
            var remainingTime = GameManager.Instance.GetRemainingTime();
            var remainingSeconds = remainingTime.TotalSeconds;
            
            if (remainingSeconds <= Main.Instance.Configuration.Instance.DeployLimitSeconds)
            {
                UnturnedChat.Say(caller, "You can't deploy right now, the game is ending soon.");
                return;
            }

            GameManager.Instance.PlayerJoinedGame(player);

            TeamManager.Instance.GroupPlayer(player);
            SpawnManager.Instance.RespawnPlayer(player);
            UnturnedChat.Say(caller, "Deployed to game");
        }
    }
}