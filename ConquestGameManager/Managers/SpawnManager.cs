using System;
using System.Linq;
using System.Threading;
using ConquestGameManager.Webhook;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using Random = System.Random;

namespace ConquestGameManager.Managers
{
    public class SpawnManager
    {
        private static SpawnManager instance;
        public static SpawnManager Instance
        {
            get { return instance ??= new SpawnManager(); }
        }

        private SpawnManager()
        {

        }

        public void RespawnPlayer(UnturnedPlayer player)
        {
            const byte angle = 0;
            
            var remainingTime = GameManager.Instance.GetRemainingTime();
            var remainingSeconds = remainingTime.TotalSeconds;

            if (GameManager.ActivePlayers.Contains(player) && remainingSeconds > Main.Instance.Configuration.Instance.RespawnLimitSeconds)
            {  
                var map = Main.Instance.Configuration.Instance.Maps.FirstOrDefault(k => k.MapID == GameManager.Instance.currentMap);
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
                        var random = new Random();
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
                            var random = new Random();
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
                ReturnToLobby(player);
            }
        }
        
        public void ReturnToLobby(UnturnedPlayer player)
        {
            TaskDispatcher.QueueOnMainThread(() =>
            {
                if (Main.Instance.Configuration.Instance.LoggingEnabled)
                {
                    Logger.Log($"{player} is not an active player, returning them to the lobby");
                }
                player.Player.teleportToLocationUnsafe(new Vector3(Main.Instance.Configuration.Instance.LobbyX, Main.Instance.Configuration.Instance.LobbyY, Main.Instance.Configuration.Instance.LobbyZ), 0);

                var steamGroupId = (CSteamID)1;
                var group = GroupManager.getOrAddGroup(steamGroupId, "Inactive Players Group", out var wasCreated);

                if (!wasCreated)
                {
                    group.name = "Inactive Players Group";
                    GroupManager.sendGroupInfo(group);
                }

                player.Player.quests.ServerAssignToGroup(steamGroupId, EPlayerGroupRank.MEMBER, true);

                player.Teleport(new Vector3(Main.Instance.Configuration.Instance.LobbyX, Main.Instance.Configuration.Instance.LobbyY, Main.Instance.Configuration.Instance.LobbyZ), 0);
                UnturnedChat.Say(player, "You have been returned to the lobby!");
            });
            
            GameManager.ActivePlayers.Remove(player);
            
        }
    }
}
