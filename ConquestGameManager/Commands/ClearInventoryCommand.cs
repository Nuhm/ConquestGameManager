using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;

namespace ConquestGameManager.Commands
{
    internal class ClearInventoryCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "ci";
        public string Help => "Command to clear the player inventory";
        public string Syntax => "/ci";
        public List<string> Aliases => new();
        public List<string> Permissions => new() { "cq.player" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (caller is not UnturnedPlayer player) return;
            player.Inventory.ClearInventory();
            Utility.Say(player, "Your inventory has been cleared.");
        }
    }
}