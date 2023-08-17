using KevunsGameManager.Models;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace KevunsGameManager.Managers
{
    public class SpawnManager
    {
        private static SpawnManager instance;
        public static SpawnManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SpawnManager();
                }
                return instance;
            }
        }

        private SpawnManager()
        {

        }

        public void RespawnPlayer(UnturnedPlayer player)
        {
            var currentMap = 1; // Example: Get the current map ID
            byte angle = 0;

            if (GameManager.Instance.ActivePlayers.Contains(player))
            {
                var map = Main.Instance.Configuration.Instance.Maps.FirstOrDefault(k => k.MapID == currentMap);
                if (map != null)
                {
                    var cooldownDuration = TimeSpan.FromSeconds(Main.Instance.Configuration.Instance.CooldownDurationSeconds);
                    var now = DateTime.UtcNow;

                    var availableLocations = map.Locations
                        .Where(location => now - location.LastUsed >= cooldownDuration)
                        .ToList();

                    // Remove locations that were used within the cooldown duration
                    availableLocations.RemoveAll(location => now - location.LastUsed < cooldownDuration);

                    if (availableLocations.Count > 0)
                    {
                        var random = new System.Random();
                        var randomLocation = availableLocations[random.Next(availableLocations.Count)];

                        player.Player.teleportToLocationUnsafe(new Vector3(randomLocation.LocationX, randomLocation.LocationY, randomLocation.LocationZ), angle);
                        randomLocation.LastUsed = now;

                        if (Main.Instance.Configuration.Instance.LoggingEnabled)
                        {
                            Logger.Log($"Player respawn at spawn point {randomLocation.LocationID} on {map.MapName}.");
                        }
                    }
                    else
                    {
                        // If all locations are on cooldown, choose a random location from the least recently used locations (lowest 3)
                        var leastRecentlyUsedLocations = map.Locations.OrderBy(location => location.LastUsed).Take(3).ToList();

                        if (leastRecentlyUsedLocations.Count > 0)
                        {
                            var random = new System.Random();
                            var randomLeastRecentlyUsedLocation = leastRecentlyUsedLocations[random.Next(leastRecentlyUsedLocations.Count)];

                            player.Player.teleportToLocationUnsafe(new Vector3(randomLeastRecentlyUsedLocation.LocationX, randomLeastRecentlyUsedLocation.LocationY, randomLeastRecentlyUsedLocation.LocationZ), angle);
                            randomLeastRecentlyUsedLocation.LastUsed = now;

                            if (Main.Instance.Configuration.Instance.LoggingEnabled)
                            {
                                Logger.Log($"Player respawn at spawn point {randomLeastRecentlyUsedLocation.LocationID} on {map.MapName} (fallback).");
                            }
                        }
                        else
                        {
                            Utility.Say(player, "No least recently used locations available.");
                        }
                    }
                }
                else
                {
                    Utility.Say(player, "Current map not found in configuration.");
                }
            }
            else
            {
                if (Main.Instance.Configuration.Instance.LoggingEnabled)
                {
                    Logger.Log($"{player} is not an active player, returning them to the lobby");
                }
                player.Player.teleportToLocationUnsafe(new Vector3(Main.Instance.Configuration.Instance.lobbyX, Main.Instance.Configuration.Instance.lobbyY, Main.Instance.Configuration.Instance.lobbyZ), 0);
            }

        }
    }
}
