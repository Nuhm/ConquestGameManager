using HarmonyLib;
using KevunsGameManager.Managers;
using KevunsGameManager.Models;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Rocket.Core.Utils;
using System.Collections.Generic;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.API;
using Rocket.Core;
using Steamworks;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace KevunsGameManager
{
    public class Main : RocketPlugin<Config>
    {
        protected override void Load()
        {
            Instance = this;

            DatabaseManager = new DatabaseManager();

            U.Events.OnPlayerConnected += EventOnConnect;
            U.Events.OnPlayerDisconnected += EventOnDisconnect;
            UnturnedPlayerEvents.OnPlayerDead += EventOnDeath;
            UnturnedPlayerEvents.OnPlayerRevive += EventOnRevive;

            Harmony harmony = new Harmony("KevunsGameManager");
            harmony.PatchAll(Assembly);

            Logger.Log("KevunsGameManager has been loaded");
        }

        protected override void Unload()
        {
            Instance = null;

            U.Events.OnPlayerConnected -= EventOnConnect;
            U.Events.OnPlayerDisconnected -= EventOnDisconnect;
            UnturnedPlayerEvents.OnPlayerDead -= EventOnDeath;
            UnturnedPlayerEvents.OnPlayerRevive -= EventOnRevive;

            foreach (var player in Provider.clients)
            {
                DatabaseManager.ChangeLastJoin(player.playerID.steamID, DateTime.UtcNow);
            }

            Logger.Log("KevunsGameManager has been unloaded");
        }

        private void EventOnConnect(UnturnedPlayer player)
        {
            ThreadPool.QueueUserWorkItem(async (o) =>
            {
                await DatabaseManager.AddPlayerAsync(player.CSteamID, player.DisplayName);

                TaskDispatcher.QueueOnMainThread(() =>
                {
                    GamePlayer gPlayer = DatabaseManager.Data.FirstOrDefault(k => k.SteamID == player.CSteamID);
                });
            });
        }

        private void EventOnDisconnect(UnturnedPlayer player)
        {
            DatabaseManager.ChangeLastJoin(player.CSteamID, DateTime.UtcNow);
        }

        private void EventOnDeath(UnturnedPlayer player, Vector3 position)
        {

        }

        private void EventOnRevive(UnturnedPlayer player, Vector3 position, byte angle)
        {
            var currentMap = 1; // Example: Get the current map ID

            var map = Main.Instance.Configuration.Instance.Maps.FirstOrDefault(k => k.MapID == currentMap);
            if (map != null)
            {
                var cooldownDuration = TimeSpan.FromSeconds(Main.Instance.Configuration.Instance.CooldownDurationSeconds);
                var now = DateTime.UtcNow;

                var availableLocations = map.Locations
                    .Where(location => now - location.LastUsed >= cooldownDuration)
                    .ToList();

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

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "Player_Connected", "[color=green]{0} has connected to the server[/color]" },
            { "Player_Disconnected", "[color=green]{0} has disconnected from the server[/color]" },
            { "Id_Wrong", "[color=red]ID should be an integer![/color]" },
            { "Added_Location", "[color=green]Successfully added spawn to {0} with ID {1}[/color]" },
            { "Updated_Location", "[color=green]Successfully updated spawn on {0} with ID {1}[/color]" }
        };

        public DatabaseManager DatabaseManager { get; set; }
        public static Main Instance { get; set; }
    }
}
