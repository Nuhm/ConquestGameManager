using HarmonyLib;
using ConquestGameManager.Models;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Commands;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;

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
                if (player is not UnturnedPlayer uply) return true;

                var gamePlayer = Main.Instance.DatabaseManager.Data.FirstOrDefault(k => k.SteamID == uply.CSteamID);
                if (gamePlayer == null)
                {
                    return true;
                }

                if (player.IsAdmin)
                {
                    return true;
                }

                var cmd = command.ToLower();

                if (cmd.StartsWith("/kits"))
                {
                    if (gamePlayer.Kits == null)
                    {
                        Utility.Say(player, Main.Instance.Translate("No_Kits").ToRich());
                        return false;
                    }

                    if (gamePlayer.Kits == null) return false;
                    if (gamePlayer.Kits.Count == 0) return false;
                    UnturnedChat.Say(player, "Kits:");
                    UnturnedChat.Say(player, gamePlayer.KitsMsg);
                    return false;
                }
                else if (cmd.StartsWith("/kit"))
                {
                    if (gamePlayer.Kits == null)
                    {
                        Utility.Say(player, Main.Instance.Translate("No_Kits").ToRich());
                        return false;
                    }
                    var message = cmd.TrimStart('/');
                    var args = message.Split(new char[]
                    {
                    ' '
                    });
                    if (args.Length != 2)
                    {
                        return false;
                    }
                    var totalKits = new List<Kit>();
                    if (totalKits == null) throw new ArgumentNullException(nameof(totalKits));
                    if (gamePlayer.Kits != null)
                    {
                        if (gamePlayer.Kits.Contains(args[1]))
                        {
                            return true;
                        }
                    }

                    var kit = totalKits.FirstOrDefault(k => string.Equals(k.KitName, args[1], StringComparison.CurrentCultureIgnoreCase));
                    if (kit != null)
                    {
                        if (gamePlayer.LastKitClaim.TryGetValue(kit, out var cooldown))
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

                        var console = new ConsolePlayer();

                        if (kit.WipeInventoryWhenClaim)
                            uply.Player.inventory.ClearInventory();

                        R.Commands.Execute(console, $"kit {kit.KitName} {uply.CSteamID}");
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