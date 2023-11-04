using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Logging;
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
        public List<string> Aliases => new();
        public List<string> Permissions => new() { "cq.player" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (caller is not UnturnedPlayer player) return;

            var permissionsToCheck = R.Commands.Commands.Select(cmd => $"{cmd.Name}").ToList();

            var accessibleCommands = permissionsToCheck
                .Where(permission => player.HasPermission(permission))
                .ToList();

            UnturnedChat.Say(player,
                accessibleCommands.Count > 0
                    ? $"Accessible commands: {string.Join(", ", accessibleCommands)}"
                    : "You don't have access to any commands.");

            // Add debugging output to console
            Logger.Log($"Permissions for {player.CharacterName}: {string.Join(", ", permissionsToCheck)}");
            Logger.Log($"Accessible Commands for {player.CharacterName}: {string.Join(", ", accessibleCommands)}");
        }
    }
}