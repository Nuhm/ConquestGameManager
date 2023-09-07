using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;

namespace ConquestGameManager.Commands
{
    internal class CommandsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "commands";
        public string Help => "List all accessible commands for the player.";
        public string Syntax => "/commands";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (caller is not UnturnedPlayer player) return;

            var accessibleCommands = (from rocketCommand in R.Commands.Commands where player.HasPermission($"rocket.{rocketCommand.Name}") select rocketCommand.Name).ToList();
            
            UnturnedChat.Say(player,
                accessibleCommands.Count > 0
                    ? $"Accessible commands: {string.Join(", ", accessibleCommands)}"
                    : "You don't have access to any commands.");
        }
    }
}