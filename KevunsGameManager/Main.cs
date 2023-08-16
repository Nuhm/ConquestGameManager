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

namespace KevunsGameManager
{
    public class Main : RocketPlugin<Config>
    {
        protected override void Load()
        {
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
                await DatabaseManager.AddPlayerAsync(player.CSteamID, player.CharacterName);

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

        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "Player_Connected", "[color=green]{0} has connected to the server[/color]" },
            { "Player_Disconnected", "[color=green]{0} has disconnected from the server[/color]" },
            { "Id_Wrong", "[color=red]ID should be an integer![/color]" },
            { "SetSpawn_Success", "[color=green]Successfully set map spawn[/color]" }
        };


        public DatabaseManager DatabaseManager { get; set; }
        public static Main Instance { get; set; }
    }
}
