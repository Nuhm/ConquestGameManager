using Rocket.API;
using Rocket.Core.Steam;
using Rocket.Core;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KevunsGameManager.Models
{
    public class GamePlayer
    {
        public CSteamID SteamID { get; set; }

        public string Username { get; set; }

        public DateTime LastJoined { get; set; }


        public GamePlayer(CSteamID steamID, string username, DateTime lastJoined)
        {
            SteamID = steamID;

            Username = username;

            LastJoined = lastJoined;
        }

        public void UpdateValue(string coloumnName, int id)
        {
            switch (coloumnName.ToLower())
            {
                default:
                    break;
            }
        }

        public void UpdateDateTime(string coloumnName, DateTime dateTime)
        {
            switch (coloumnName.ToLower())
            {
                case "last joined":
                    LastJoined = dateTime;
                    break;
                default:
                    break;
            }
        }
    }
}
