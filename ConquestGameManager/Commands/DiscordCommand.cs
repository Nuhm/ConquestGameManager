using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;

namespace ConquestGameManager.Commands
{
    internal class DiscordCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "discord";
        public string Help => "Command to get a link to the discord";
        public string Syntax => "/discord";
        public List<string> Aliases => new();
        public List<string> Permissions => new() { "cq.player" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (caller is UnturnedPlayer player) Utility.OpenUrl(player, "Join Our Discord!", "https://discord.gg/kxrkhaNd4T");
        }
    }
}


