using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;

namespace ConquestGameManager.Commands
{
    internal class HelpCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "help";
        public string Help => "Command to get a link to help documents";
        public string Syntax => "/help";
        public List<string> Aliases => new();
        public List<string> Permissions => new() { "cq.player" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (caller is UnturnedPlayer player) Utility.OpenUrl(player, "For Help Join Our Discord!", "https://discord.gg/kxrkhaNd4T");
        }
    }
}