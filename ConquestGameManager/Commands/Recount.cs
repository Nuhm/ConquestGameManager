using Rocket.API;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using System.Linq;
using SDG.Unturned;

namespace ConquestGameManager.Commands
{
    class RecountCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "recount";

        public string Help => "Recount kits for a player";

        public string Syntax => "/recount (Name)";

        public List<string> Aliases => new();

        public List<string> Permissions => new();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                foreach (var gamePlayer in Provider.clients.Select(player => player.playerID.steamID).Select(steamID => Main.Instance.DatabaseManager.Data.FirstOrDefault(k => k.SteamID == steamID)))
                {
                    gamePlayer?.BuildKits();
                }
                UnturnedChat.Say(caller, "Recounted all kits for the player, they should have access to all the kits they have perms for");
            }
            else if (!Utility.TryGetPlayer(command[0], out var steamID))
            {
                Utility.Say(caller, Main.Instance.Translate("Player_Not_Found").ToRich());
            }
            else
            {
                var gamePlayer = Main.Instance.DatabaseManager.Data.FirstOrDefault(k => k.SteamID == steamID);
                gamePlayer?.BuildKits();
                UnturnedChat.Say(caller, "Recounted all the kits for the player, they should have access to all the kits they have perms for");
            }
        }
    }
}