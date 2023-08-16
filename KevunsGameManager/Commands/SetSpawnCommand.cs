using Rocket.API;
using Rocket.Unturned.Chat;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Rocket.Unturned.Player;
using System.Linq;

namespace KevunsGameManager.Commands
{
    internal class SetSpawnCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "setspawn";

        public string Help => "Command to set map spawns";

        public string Syntax => "/setspawn (Map ID) (Location ID)";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = caller as UnturnedPlayer;

            if (command.Length != 1)
            {
                UnturnedChat.Say(caller, $"Correct Usage: {Syntax}", Color.red);
                return;
            }

            if (!int.TryParse(command[0], out int mapID))
            {
                Utility.Say(caller, Main.Instance.Translate("Id_Wrong").ToRich());
                return;
            }

            if (!int.TryParse(command[1], out int locationID))
            {
                Utility.Say(caller, Main.Instance.Translate("Id_Wrong").ToRich());
                return;
            }

            var map = Main.Instance.DatabaseManager.MapData.FirstOrDefault(k => k.MapID == int.Parse(command[0])); // Checks if map exists in DB
            if (map == null)
            {
                Utility.Say(caller, Main.Instance.Translate("Map_Not_Found", mapID));
                return;
            }

            // Need logic to put locations in database under map ID
            // Need prior logic to add columns for different maps with primary keys

            var location = Main.Instance.DatabaseManager.MapData.FirstOrDefault(k => k.LocationID == int.Parse(command[1])); // Searches DB for location ID

            Utility.Say(caller, Main.Instance.Translate("SetSpawn_Success").ToRich());
        }
    }
}
