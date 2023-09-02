using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace KevunsGameManager.Commands
{
    internal class SudoCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "sudo";
        public string Help => "Command to execute commands on behalf of another";
        public string Syntax => "/sudo [player | *] [command]";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>();
        [Obsolete("Obsolete")]
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, $"Correct Usage: {Syntax}", Color.red);
                return;
            }
            
            var message = string.Concat(command[1]);

            if (command[0].Equals("*"))
            {
                foreach (var player in Provider.clients)
                {
                    if (player != null)
                    {
                        CSteamID steamID = UnturnedPlayer.FromSteamPlayer(player).CSteamID;
                        ChatManager.instance.askChat(steamID, (byte)EChatMode.GLOBAL, message);
                    }
                }
            }
            else
            {
                UnturnedPlayer player = UnturnedPlayer.FromName(command[0]);
                if (player != null)
                {
                    CSteamID steamID = player.CSteamID;
                    ChatManager.instance.askChat(steamID, (byte) EChatMode.GLOBAL, message);
                }
                else
                {
                    UnturnedChat.Say(caller, "Failed to find player", Color.red);
                    return;
                }
            }
            
            UnturnedChat.Say(caller, "Successfully message/command on behalf of player");
        }
    }
}