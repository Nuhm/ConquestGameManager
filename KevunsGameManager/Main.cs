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

            Logger.Log($"Loaded {Configuration.Instance.Maps.Count} maps");
            Logger.Log($"Loaded {Configuration.Instance.Gamemodes.Count} gamemodes");

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
            SpawnManager.Instance.RespawnPlayer(player);
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
            SpawnManager.Instance.RespawnPlayer(player);
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "Player_Connected", "[color=green]{0} has connected to the server[/color]" },
            { "Player_Disconnected", "[color=green]{0} has disconnected from the server[/color]" },
            { "Id_Wrong", "[color=red]ID should be an integer![/color]" },
            { "Added_Location", "[color=green]Successfully added spawn to {0} with ID {1}[/color]" },
            { "Updated_Location", "[color=green]Successfully updated spawn on {0} with ID {1}[/color]" },
            { "Amended_Location", "[color=yellow]Amended spawn ID to {0}[/color]" }
        };

        public DatabaseManager DatabaseManager { get; set; }
        public static Main Instance { get; set; }
    }
}
