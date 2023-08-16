using Rocket.API;
using Rocket.Unturned.Chat;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Rocket.Unturned.Player;
using System.Linq;
using Rocket.Core.Steam;
using KevunsGameManager.Models;

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

            if (command.Length != 2)
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

            var map = Main.Instance.Configuration.Instance.Maps.FirstOrDefault(k => k.MapID == int.Parse(command[0])); // Checks if map exists in config
            if (map == null)
            {
                Utility.Say(caller, Main.Instance.Translate("Map_Not_Found", mapID));
                return;
            }

            var location = map.Locations.FirstOrDefault(k => k.LocationID == locationID);
            if (location == null)
            {
                // Create a new location and add it to the map's Locations list
                var newLocation = new Location
                {
                    LocationID = locationID,
                    HasCooldown = false, // You might adjust this based on your requirements
                    LocationX = player.Position.x,
                    LocationY = player.Position.y,
                    LocationZ = player.Position.z
                };

                map.Locations.Add(newLocation);

                // Save the modified configuration
                Main.Instance.Configuration.Save();

                Utility.Say(caller, Main.Instance.Translate("Added_Location", map.MapName, locationID).ToRich());

                return;
            }

            location.LocationX = player.Position.x;
            location.LocationY = player.Position.y;
            location.LocationZ = player.Position.z;

            Main.Instance.Configuration.Save();

            Utility.Say(caller, Main.Instance.Translate("Updated_Location", map.MapName, locationID).ToRich());
        }
    }
}
