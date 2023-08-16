using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KevunsGameManager
{
    public static class Utility
    {
        public static string ToRich(this string value)
        {
            return value.Replace('[', '<').Replace(']', '>');
        }

        public static void ClearInventory(this PlayerInventory inv)
        {
            inv.player.equipment.sendSlot(0);
            inv.player.equipment.sendSlot(1);

            for (byte page = 0; page < PlayerInventory.PAGES - 1; page++)
            {
                for (int index = inv.getItemCount(page) - 1; index >= 0; index--)
                {
                    inv.removeItem(page, (byte)index);
                }
            }

            inv.player.clothing.updateClothes(0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0]);
            inv.player.equipment.sendSlot(0);
            inv.player.equipment.sendSlot(1);
        }

        public static void Say(IRocketPlayer target, string message)
        {
            if (target is UnturnedPlayer player)
            {
                ChatManager.serverSendMessage(message, Color.green, toPlayer: player.SteamPlayer(),
                    useRichTextFormatting: true);
            }
        }

        public static string ToUnrich(this string value)
        {
            string newString = "";
            bool omit = false;

            foreach (var c in value)
            {
                if (c == '[')
                {
                    omit = true;
                    continue;
                }
                else if (c == ']')
                {
                    omit = false;
                    continue;
                }

                if (omit) continue;
                newString += c;
            }
            return newString;
        }
    }
}
