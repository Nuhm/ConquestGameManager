using HarmonyLib;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Commands;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using ConquestGameManager.Models;

namespace ConquestGameManager.Events
{
    public class RocketExecuteCommand
    {
        [HarmonyPatch(typeof(RocketCommandManager), "Execute")]
        public static class ExecuteCommand_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(IRocketPlayer player, string command)
            {
                UnturnedPlayer uply = player as UnturnedPlayer;
                if (uply == null) return true;

                GamePlayer gamePlayer = Main.Instance.DatabaseManager.Data.FirstOrDefault(k => k.SteamID == uply.CSteamID);
                if (gamePlayer == null)
                {
                    return true;
                }

                if (player.IsAdmin)
                {
                    return true;
                }

                string cmd = command.ToLower();

                if (cmd.StartsWith("/kits"))
                {
                    if (gamePlayer.RankKits == null && gamePlayer.CustomKits == null)
                    {
                        Utility.Say(player, Main.Instance.Translate("No_Kits").ToRich());
                        return false;
                    }

                    if (gamePlayer.RankKits != null)
                    {
                        if (gamePlayer.RankKits.Count != 0)
                        {
                            UnturnedChat.Say(player, "Rank Kits:");
                            UnturnedChat.Say(player, string.Join(", ", gamePlayer.RankKits.Select(kit => kit.KitName)));
                        }
                    }

                    if (gamePlayer.CustomKits != null)
                    {
                        if (gamePlayer.CustomKits.Count != 0)
                        {
                            UnturnedChat.Say(player, "Custom Kits:");
                            UnturnedChat.Say(player, string.Join(", ", gamePlayer.CustomKits.Select(kit => kit.KitName)));
                        }
                    }
                    return false;
                }
                else if (cmd.StartsWith("/kit"))
                {
                    if (gamePlayer.RankKits == null && gamePlayer.CustomKits == null)
                    {
                        Utility.Say(player, Main.Instance.Translate("No_Kits").ToRich());
                        return false;
                    }
                    string message = cmd.TrimStart('/');
                    string[] args = message.Split(new char[] { ' ' });
                    if (args.Length != 2)
                    {
                        return false;
                    }
                    List<Kit> totalKits = new List<Kit>();
                    if (gamePlayer.CustomKits != null)
                    {
                        if (gamePlayer.CustomKits.Any(kit => kit.KitName.Equals(args[1], StringComparison.OrdinalIgnoreCase)))
                        {
                            return true;
                        }
                    }
                    if (gamePlayer.RankKits != null)
                    {
                        totalKits.AddRange(gamePlayer.RankKits);
                    }

                    Kit kit = totalKits.FirstOrDefault(k => k.KitName.Equals(args[1], StringComparison.OrdinalIgnoreCase));
                    if (kit != null)
                    {
                        bool isRankKit = gamePlayer.RankKits != null && gamePlayer.RankKits.Contains(kit);

                        if (gamePlayer.LastKitClaim.TryGetValue(kit, out DateTime cooldown))
                        {
                            if ((DateTime.UtcNow - cooldown).TotalSeconds < kit.KitCooldownSeconds)
                            {
                                Utility.Say(player, Main.Instance.Translate("Kit_Cooldown", kit.KitCooldownSeconds).ToRich());
                                return false;
                            }
                            gamePlayer.LastKitClaim.Remove(kit);
                        }

                        if (kit.HasCooldown)
                            gamePlayer.LastKitClaim.Add(kit, DateTime.UtcNow);

                        ConsolePlayer console = new ConsolePlayer();

                        if (kit.WipeInventoryWhenClaim)
                            uply.Player.inventory.ClearInventory();

                        R.Commands.Execute(console, $"kit {(isRankKit ? "" : "")}{kit.KitName} {uply.CSteamID}");
                        gamePlayer.LastUsedKit = kit;
                    }
                    else
                    {
                        Utility.Say(player, Main.Instance.Translate("Wrong_Or_No_Access_Kit").ToRich());
                    }
                    return false;
                }
                return true;
            }
        }
    }
}
