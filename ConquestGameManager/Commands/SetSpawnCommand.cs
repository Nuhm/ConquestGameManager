using System.Collections.Generic;
using System.Linq;
using ConquestGameManager.Models;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using UnityEngine;

namespace ConquestGameManager.Commands
{
    internal class SetSpawnCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "setspawn";
        public string Help => "Command to set map spawns";
        public string Syntax => "/setspawn (Map ID) (Location ID)";
        public List<string> Aliases => new();
        public List<string> Permissions => new();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = caller as UnturnedPlayer;

            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, $"Correct Usage: {Syntax}", Color.red);
                return;
            }

            if (!int.TryParse(command[0], out var mapID))
            {
                Utility.Say(caller, Main.Instance.Translate("Id_Wrong").ToRich());
                return;
            }

            if (!int.TryParse(command[1], out var locationID))
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
                var newLocationID = map.Locations.Max(existingLocation => existingLocation.LocationID) + 1;

                if (player != null)
                {
                    var newLocation = new Location
                    {
                        LocationID = newLocationID,
                        HasCooldown = false,
                        LocationX = player.Position.x,
                        LocationY = player.Position.y,
                        LocationZ = player.Position.z
                    };

                    map.Locations.Add(newLocation);
                }

                if (int.Parse(command[1]) != newLocationID)
                {
                    Utility.Say(caller, Main.Instance.Translate("Amended_Location", newLocationID).ToRich());
                }


                Utility.Say(caller, Main.Instance.Translate("Added_Location", map.MapName, newLocationID).ToRich());
            }
            else
            {
                if (player != null)
                {
                    location.LocationX = player.Position.x;
                    location.LocationY = player.Position.y;
                    location.LocationZ = player.Position.z;
                }

                Utility.Say(caller, Main.Instance.Translate("Updated_Location", map.MapName, locationID).ToRich());
            }
            Main.Instance.Configuration.Save();
        }
    }
}
