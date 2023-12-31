﻿using System.Collections.Generic;
using ConquestGameManager.Models;
using Rocket.API;

namespace ConquestGameManager
{
    public class Config : IRocketPluginConfiguration
    {
        public bool LoggingEnabled { get; set; }
        public string ConnectionString { get; set; }

        public int CooldownDurationSeconds { get; set; }

        public float LobbyX { get; set; }
        public float LobbyY { get; set; }
        public float LobbyZ { get; set; }

        public List<Map> Maps { get; set; }
        public List<Gamemode> Gamemodes { get; set; }
        public List<Rank> Ranks { get; set; }
        public int DeployLimitSeconds { get; set; }
        public int RespawnLimitSeconds { get; set; }
        public string DeployWebhook { get; set; }
        public string GameInfoWebhook { get; set; }
        public int KillXp { get; set; }
        public int HeadshotBonusXp { get; set; }


        public void LoadDefaults()
        {
            ConnectionString = "server=localhost;user=root;database=unturned;port=3306;password=root";
            CooldownDurationSeconds = 5;

            LobbyX = 0;
            LobbyY = 0;
            LobbyZ = 0;

            Maps = new List<Map>
            {
                new Map(1, "Test", 0, 40, new List<Time>
                {
                    new Time(0.2f, 0.4f, 0.2f, 0.2f)
                }, new List<Location>
                {
                    new Location(1, false, 0, 0, 0),
                    new Location(2, false, 0, 0, 0),
                    new Location(3, false, 0, 0, 0),
                }, true),
                new Map(2, "Warehouse", 0, 40, new List<Time>
                {
                    new Time(0.2f, 0.4f, 0.2f, 0.2f)
                }, new List<Location>
                {
                    new Location(1, false, 0, 0, 0),
                    new Location(2, false, 0, 0, 0),
                    new Location(3, false, 0, 0, 0),
                }, true),
                new Map(3, "Bunker", 0, 40, new List<Time>
                {
                    new Time(0.2f, 0.4f, 0.2f, 0.2f)
                }, new List<Location>
                {
                    new Location(1, false, 0, 0, 0),
                    new Location(2, false, 0, 0, 0),
                    new Location(3, false, 0, 0, 0),
                }, true),
            };
            Gamemodes = new List<Gamemode>
            {
                new Gamemode
                {
                    ID = 0,
                    Name = "FFA",
                    Duration = 20,
                    HasTeams = false,
                    IsEnabled = true,
                },
                new Gamemode
                {
                    ID = 1,
                    Name = "TDM",
                    Duration = 20,
                    HasTeams = true,
                    IsEnabled = true
                }
            };
            Ranks = new List<Rank>
            {
                new Rank
                {
                    RankID = 0,
                    RankName = "Lvl 1", 
                    RequiredXp = 0, 
                    HealthLevel = 0,
                    MovementLevel = 0,
                    JumpLevel = 0,
                    StaminaLevel = 0,
                    RankKits = new List<Kit>
                    {
                        new Kit("Kit11", 0, false, true, false),
                        new Kit("Kit12", 30, false, false, true)
                    }
                },
                new Rank
                {
                    RankID = 1,
                    RankName = "Lvl 2", 
                    RequiredXp = 1000, 
                    HealthLevel = 0,
                    MovementLevel = 0,
                    JumpLevel = 0,
                    StaminaLevel = 0,
                    RankKits = new List<Kit>
                    {
                        new Kit("Kit21", 0, false, true, false),
                        new Kit("Kit22", 30, false, false, true)
                    }
                },
            };


            DeployLimitSeconds = 10;
            RespawnLimitSeconds = 10;
            DeployWebhook = "";
            GameInfoWebhook = "";
            KillXp = 100;
            HeadshotBonusXp = 25;
        }
    }
}
