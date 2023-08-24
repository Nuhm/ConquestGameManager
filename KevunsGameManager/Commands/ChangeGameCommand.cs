using Rocket.API;
using Rocket.Core.Steam;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using KevunsGameManager.Managers;
using UnityEngine;

namespace KevunsGameManager.Commands
{
    internal class ChangeGameCommad : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "changegame";

        public string Help => "Change the game mode.";

        public string Syntax => "/changegame <mode>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = caller as UnturnedPlayer;

            if (command.Length < 1)
            {
                UnturnedChat.Say(caller, "Usage: /changegame <mode>");
                return;
            }

            string newMode = command[0]; // Get the specified mode

            // Logic to change the game mode using GameManager
            if (GameManager.Instance.ChangeGameMode(player, newMode))
            {
                UnturnedChat.Say(caller, $"Switched to {newMode} game mode!");
            }
            else
            {
                UnturnedChat.Say(caller, $"Invalid game mode: {newMode}");
            }
        }
    }
}