#nullable enable
using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Text.RegularExpressions;
using Rocket.Core.Steam;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace KevunsGameManager
{
    public static class Utility
    {
        public static void Say(object target, string message)
        {
            var updatedMessage = message.ToRich();
            switch (target)
            {
                case UnturnedPlayer uPlayer:
                    ChatManager.serverSendMessage(updatedMessage, Color.green, toPlayer: uPlayer.SteamPlayer(), useRichTextFormatting: true);
                    break;
                case Player player:
                    ChatManager.serverSendMessage(updatedMessage, Color.green, toPlayer: player.channel.owner, useRichTextFormatting: true);
                    break;
                case SteamPlayer sPlayer:
                    ChatManager.serverSendMessage(updatedMessage, Color.green, toPlayer: sPlayer, useRichTextFormatting: true);
                    break;
                case ConsolePlayer:
                    Logger.Log(updatedMessage.ToUnrich());
                    break;
            }
        }
        
        public static void Broadcast(string message, string? iconURL = null)
        {
            var updatedMessage = message.ToRich();
            ChatManager.serverSendMessage(updatedMessage, Color.green, useRichTextFormatting: true, iconURL: iconURL);
        }
        
        public static void ClearInventory(this PlayerInventory inv)
        {
            for (byte page = 0; page < PlayerInventory.PAGES - 2; page++)
            for (var index = inv.getItemCount(page) - 1; index >= 0; index--)
                inv.removeItem(page, (byte)index);

            inv.player.clothing.updateClothes(0, 0, Array.Empty<byte>(), 0, 0, Array.Empty<byte>(), 0, 0, Array.Empty<byte>(), 0, 0, Array.Empty<byte>(), 0, 0, Array.Empty<byte>(), 0, 0, Array.Empty<byte>(), 0, 0, Array.Empty<byte>());
            inv.player.equipment.sendSlot(0);
            inv.player.equipment.sendSlot(1);
        }

        public static string ToUnrich(this string value) => new Regex(@"<[^>]*>", RegexOptions.IgnoreCase).Replace(value, "").Trim();
        public static string ToRich(this string value) => value.Replace('[', '<').Replace(']', '>').Replace("osqb", "[").Replace("csqb", "]");
        public static string ToSquareBrackets(this string value) => value.Replace("[", "osqb").Replace("]", "csqb");
    }
}
