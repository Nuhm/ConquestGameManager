using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using UnityEngine;

namespace KevunsGameManager.Commands
{
    internal class DiscordCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "discord";

        public string Help => "Command to get a link to the discord";

        public string Syntax => "/discord";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = caller as UnturnedPlayer;
            UnturnedChat.Say("This feature is yet to be added", Color.yellow);
            //player.sendBrowserRequest("Our Discord:", "https://discord.com/");
        }
    }
}


