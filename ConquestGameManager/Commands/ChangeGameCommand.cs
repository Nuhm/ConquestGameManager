using System.Collections.Generic;
using ConquestGameManager.Managers;
using Rocket.API;
using Rocket.Unturned.Chat;

namespace ConquestGameManager.Commands
{
    internal class ChangeGameCommad : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "changegame";
        public string Help => "Change the game mode.";
        public string Syntax => "/changegame <mode>";
        public List<string> Aliases => new();
        public List<string> Permissions => new() { "cq.admin" };
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                UnturnedChat.Say(caller, "Usage: /changegame <mode>");
                return;
            }

            var newMode = command[0]; // Get the specified mode

            // Logic to change the game mode using GameManager
            UnturnedChat.Say(caller,
                GameManager.Instance.ChangeGameMode(newMode)
                    ? $"Switched to {newMode} game mode!"
                    : $"Invalid game mode: {newMode}");
        }
    }
}