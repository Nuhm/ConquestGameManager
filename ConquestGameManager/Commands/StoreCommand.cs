using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;

namespace ConquestGameManager.Commands
{
    internal class StoreCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "store";
        public string Help => "Command to get a link to the store";
        public string Syntax => "/store";
        public List<string> Aliases => new();
        public List<string> Permissions => new();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (caller is UnturnedPlayer player) Utility.OpenUrl(player, "Our store!", "https://nordicroleplay.tebex.io/category/military-roleplay");
        }
    }
}