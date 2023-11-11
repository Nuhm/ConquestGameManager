using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConquestGameManager.Managers;
using ConquestGameManager.Models;
using ConquestGameManager.Webhook;
using HarmonyLib;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Core.Utils;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace ConquestGameManager
{
    public class Main : RocketPlugin<Config>
    {
        protected override void Load()
        {
            Instance = this;

            DatabaseManager = new DatabaseManager();

            Level.onPostLevelLoaded += OnLevelLoaded;
            
            U.Events.OnPlayerConnected += EventOnConnect;
            U.Events.OnPlayerDisconnected += EventOnDisconnect;
            UnturnedPlayerEvents.OnPlayerDeath += EventOnDeath;
            UnturnedPlayerEvents.OnPlayerRevive += EventOnRevive;

            var harmony = new Harmony("ConquestGameManager");
            harmony.PatchAll(Assembly);

            Logger.Log($"Loaded {Configuration.Instance.Maps.Count} maps");
            Logger.Log($"Loaded {Configuration.Instance.Gamemodes.Count} gamemodes");

            Logger.Log("ConquestGameManager has been loaded");
        }

        protected override void Unload()
        {
            Instance = null;

            Level.onPostLevelLoaded -= OnLevelLoaded;

            U.Events.OnPlayerConnected -= EventOnConnect;
            U.Events.OnPlayerDisconnected -= EventOnDisconnect;
            UnturnedPlayerEvents.OnPlayerDeath -= EventOnDeath;
            UnturnedPlayerEvents.OnPlayerRevive -= EventOnRevive;

            foreach (var player in Provider.clients)
            {
                DatabaseManager.ChangeLastJoin(player.playerID.steamID, DateTime.UtcNow);
            }

            Logger.Log("ConquestGameManager has been unloaded");
        }

        private static void OnLevelLoaded(int level)
        {
            GameManager.Instance.Start();
            Logger.Log("Started GameManager");
        }

        private void EventOnConnect(UnturnedPlayer player)
        {
            ThreadPool.QueueUserWorkItem(async (o) =>
            {
                await DatabaseManager.AddPlayerAsync(player.CSteamID, player.DisplayName);

                TaskDispatcher.QueueOnMainThread(() =>
                {
                    var gPlayer = DatabaseManager.Data.FirstOrDefault(k => k.SteamID == player.CSteamID);
                });
            });
            DatabaseManager.StartPlaytimeTracking(player.CSteamID);;
            SpawnManager.Instance.RespawnPlayer(player);
        }

        private async void EventOnDisconnect(UnturnedPlayer player)
        {
            if (DatabaseManager.PlaytimeTracker.ContainsKey(player.CSteamID))
            {
                var playtimeSeconds = DatabaseManager.PlaytimeTracker[player.CSteamID];
        
                await DatabaseManager.UpdatePlaytimeAsync(player.CSteamID, playtimeSeconds);
        
                DatabaseManager.PlaytimeTracker.Remove(player.CSteamID);
            }
    
            DatabaseManager.ChangeLastJoin(player.CSteamID, DateTime.UtcNow);
        }


        private static void EventOnDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            var gamePlayer = Instance.DatabaseManager.Data.FirstOrDefault(k => k.SteamID == player.CSteamID);
            if (gamePlayer == null) return;
            IEnumerable<Kit> wipeCooldown = gamePlayer.LastKitClaim.Where(k => k.Key.ResetCooldownOnDie == true).Select(k => k.Key).ToList();
            foreach (var wipeKit in wipeCooldown)
            {
                gamePlayer.LastKitClaim.Remove(wipeKit);
                Logger.Log($"Cleared cooldown for {wipeKit}");
            }
            
            var killerPlayer = UnturnedPlayer.FromCSteamID(murderer);
            var killerName = killerPlayer != null ? killerPlayer.DisplayName : "Unknown";
            
            if (killerPlayer == null) return;
            
            var wasHeadshot = limb == ELimb.SKULL;
            
            var causeType = GetCauseType(cause);
            var limbType = GetLimbType(limb);
            var weaponType = GetWeaponType(killerPlayer);
            
            var distance = Math.Round(Vector3.Distance(player.Position, killerPlayer.Position), 2);
            var killerPlayerGamePlayer = Instance.DatabaseManager.Data.FirstOrDefault(k => k.SteamID == murderer);
            Instance.RankManager?.CheckAndHandleRankUp(killerPlayerGamePlayer);
            
            ThreadPool.QueueUserWorkItem(async (o) =>
            {
                await Instance.DatabaseManager.UpdateDeathCountAsync(player);
                await Instance.DatabaseManager.UpdateKillCountAsync(killerPlayer, wasHeadshot);
                
                await Instance.DatabaseManager.UpdateXPAsync(killerPlayer, wasHeadshot);
                
                var embed = new Embed(null, $"**{player.DisplayName}** was killed by **{killerName}**!", null, "16714764", DateTime.UtcNow.ToString("s"),
                    new Footer(Provider.serverName, Provider.configData.Browser.Icon),
                    new Author(null, null, null),
                    new[]
                    {
                        new Field("**Player:**", $"[**{player.DisplayName}**](https://steamcommunity.com/profiles/{player.CSteamID}/)", true),
                        new Field("**Killer:**", $"[**{killerName}**](https://steamcommunity.com/profiles/{murderer}/)", true),
                        new Field("**Cause:**", $"{causeType}", true),
                        new Field("**Limb:**", $"{limbType}", true),
                        new Field("**Weapon:**", $"{weaponType}", true),
                        new Field("**Distance:**", $"{distance}", true)
                    },
                    null, null);
                DiscordManager.SendEmbed(embed, "Death", Main.Instance.Configuration.Instance.GameInfoWebhook);
            });
                
            Logger.Log($"{player.DisplayName} was killed by {killerName} using a {weaponType} at a distance of {distance:F2} meters.");
        }

        private static readonly Dictionary<ELimb, string> LimbTypeMap = new()
        {
            { ELimb.SKULL, "Head" },
            { ELimb.SPINE, "Torso" },
            { ELimb.LEFT_FRONT, "Torso" },
            { ELimb.RIGHT_FRONT, "Torso" },
            { ELimb.LEFT_BACK, "Torso" },
            { ELimb.RIGHT_BACK, "Torso" },
            { ELimb.LEFT_ARM, "Arm" },
            { ELimb.RIGHT_ARM, "Arm" },
            { ELimb.LEFT_LEG, "Leg" },
            { ELimb.RIGHT_LEG, "Leg" },
            { ELimb.LEFT_HAND, "Hand" },
            { ELimb.RIGHT_HAND, "Hand" },
            { ELimb.LEFT_FOOT, "Foot" },
            { ELimb.RIGHT_FOOT, "Foot" },
        };

        private static readonly Dictionary<EDeathCause, string> CauseTypeMap = new()
        {
            { EDeathCause.GUN, "Gun" },
            { EDeathCause.SUICIDE, "Suicide" },
            { EDeathCause.MELEE, "Melee Weapon" },
            { EDeathCause.PUNCH, "Fist" },
            { EDeathCause.FOOD, "Hunter" },
            { EDeathCause.KILL, "Admin" },
            { EDeathCause.SHRED, "Shredding" },
            { EDeathCause.WATER, "Thirst" },
            { EDeathCause.BREATH, "Suffocation" },
            { EDeathCause.BURNER, "Burning" },
            { EDeathCause.CHARGE, "Explosives" },
            { EDeathCause.GRENADE, "Grenade" },
            { EDeathCause.MISSILE, "Missile" },
            { EDeathCause.VEHICLE, "Vehicle" },
            { EDeathCause.INFECTION, "Infection" },
        };

        private static string GetLimbType(ELimb limb)
        {
            return LimbTypeMap.TryGetValue(limb, out var limbType) ? limbType : "Unknown";
        }

        private static string GetCauseType(EDeathCause cause)
        {
            return CauseTypeMap.TryGetValue(cause, out var causeType) ? causeType : "Unknown";
        }

        private static string GetWeaponType(UnturnedPlayer killerPlayer)
        {
            if (killerPlayer != null && killerPlayer.Player != null && killerPlayer.Player.equipment != null && killerPlayer.Player.equipment.asset != null)
            {
                return killerPlayer.Player.equipment.asset.itemName;
            }
            return "Unknown";
        }
        
        private static void EventOnRevive(UnturnedPlayer player, Vector3 position, byte angle)
        {
            SpawnManager.Instance.RespawnPlayer(player);
            
            var gamePlayer = Instance.DatabaseManager.Data.FirstOrDefault(k => k.SteamID == player.CSteamID);

            var kit = gamePlayer?.LastUsedKit;
            Logger.Log($"Last used kit was {kit}");
            if (kit == null)
            {
                return;
            }
            
            if (gamePlayer.LastKitClaim.TryGetValue(kit, out var cooldown))
            {
                if ((DateTime.UtcNow - cooldown).TotalSeconds < kit.KitCooldownSeconds)
                {
                    return;
                }

                gamePlayer.LastKitClaim.Remove(kit);
            }

            if (kit.HasCooldown)
            {
                gamePlayer?.LastKitClaim.Add(kit, DateTime.UtcNow);
            }
            
            Logger.Log($"Has cooldown? {kit.HasCooldown}");

            R.Commands.Execute(new ConsolePlayer(), $"kit {kit.KitName} {player.CSteamID}");
        }

        public override TranslationList DefaultTranslations => new()
        {
            { "Player_Connected", "[color=green]{0} has connected to the server[/color]" },
            { "Player_Disconnected", "[color=green]{0} has disconnected from the server[/color]" },
            { "Id_Wrong", "[color=red]ID should be an integer![/color]" },
            { "Added_Location", "[color=green]Successfully added spawn to {0} with ID {1}[/color]" },
            { "Updated_Location", "[color=green]Successfully updated spawn on {0} with ID {1}[/color]" },
            { "Amended_Location", "[color=yellow]Amended spawn ID to {0}[/color]" }
        };

        public DatabaseManager DatabaseManager { get; set; }
        public RankManager RankManager { get; set; }
        public static Main Instance { get; private set; }
    }
}
