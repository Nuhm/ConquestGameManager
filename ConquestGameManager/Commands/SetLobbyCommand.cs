using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Player;

namespace ConquestGameManager.Commands
{
    internal class SetLobbyCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "setlobby";
        public string Help => "Command to set server lobby spawn point";
        public string Syntax => "/setlobby";
        public List<string> Aliases => new()
        {
            Capacity = 0
        };
        public List<string> Permissions => new() { "cq.admin" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (caller is UnturnedPlayer player)
            {
                Main.Instance.Configuration.Instance.LobbyX = player.Position.x;
                Main.Instance.Configuration.Instance.LobbyY = player.Position.y;
                Main.Instance.Configuration.Instance.LobbyZ = player.Position.z;
            }

            Main.Instance.Configuration.Save();

            Utility.Say(caller, "Server lobby spawn point updated.");
        }
    }
}
