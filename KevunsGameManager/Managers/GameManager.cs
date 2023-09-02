using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KevunsGameManager.Models;
using KevunsGameManager.Webhook;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using Random = UnityEngine.Random;
using Time = KevunsGameManager.Models.Time;

namespace KevunsGameManager.Managers
{
    public class GameManager
    {
        private static GameManager instance;
        public static GameManager Instance => instance ??= new GameManager();
        public List<GameMode> gameModes;
        public int currentGameModeIndex;
        public int currentMap;
        private int previousGameModeIndex = -1;
        public List<UnturnedPlayer> ActivePlayers { get; } = new List<UnturnedPlayer>();

        private GameManager()
        {
            gameModes = new List<GameMode>();
            foreach (var gamemodeConfig in Main.Instance.Configuration.Instance.Gamemodes)
            {
                gameModes.Add(new GameMode(gamemodeConfig.Name, TimeSpan.FromMinutes(gamemodeConfig.Duration)));
            }
        }

        private bool isRunning;

        public void Start()
        {
            SwitchToNextGameMode();
            Logger.Log("Started GAME!");

            isRunning = true;

            Task.Run(async () =>
            {
                while (isRunning)
                {
                    Update(TimeSpan.FromSeconds(1));
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });
        }
        
        private void Update(TimeSpan elapsedTime)
        {
            gameModes[currentGameModeIndex].Update(elapsedTime);

            if (gameModes[currentGameModeIndex].IsFinished)
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    Embed embed = new Embed(null, $"A **{GameManager.Instance.gameModes[GameManager.Instance.currentGameModeIndex].Name}** game has ended!", null, "16714764", DateTime.UtcNow.ToString("s"),
                        new Footer(Provider.serverName, Provider.configData.Browser.Icon),
                        new Author(null, null, null),
                        new Field[]
                        {
                        },
                        null, null);
                    DiscordManager.SendEmbed(embed, "Game Ended", Main.Instance.Configuration.Instance.GameInfoWebhook);
                });
            
                // Send game summary webhook here
                
                TaskDispatcher.QueueOnMainThread( () =>
                    {
                        ReturnToLobby();
                        
                        Utility.Broadcast("The game has just ended!");
                        Utility.Broadcast("A new game is starting!");

                        SwitchToNextGameMode();
                    }
                );
            }
        }

        private void ReturnToLobby()
        {
            List<UnturnedPlayer> playersToTeleport = new List<UnturnedPlayer>();

            foreach (var player in ActivePlayers)
            {
                playersToTeleport.Add(player);
            }

            foreach (var player in playersToTeleport)
            {
                TaskDispatcher.QueueOnMainThread(() =>
                {
                    if (player == null)
                        return;

                    player.Teleport(new Vector3(Main.Instance.Configuration.Instance.LobbyX, Main.Instance.Configuration.Instance.LobbyY, Main.Instance.Configuration.Instance.LobbyZ), 0);
                    UnturnedChat.Say(player, "You have been returned to the lobby!");
                });
                
                ActivePlayers.Remove(player);
            }
        }

        private void SwitchToNextGameMode()
        {
            // Choose a random enabled map
            List<Map> enabledMaps = Main.Instance.Configuration.Instance.Maps.Where(map => map.IsEnabled).ToList();
            if (enabledMaps.Count > 0)
            {
                int randomMapIndex = Random.Range(0, enabledMaps.Count);
                Map randomMap = enabledMaps[randomMapIndex];
                currentMap = randomMap.MapID;
            }
            else
            {
                currentMap = 0; // Fallback map
            }
    
            // Choose a random game mode that is not the same as the previous one
            int randomGameModeIndex = GetRandomNonRepeatingGameModeIndex();
            currentGameModeIndex = randomGameModeIndex;

            SwitchToGameMode(currentGameModeIndex);
        }
        
        private int GetRandomNonRepeatingGameModeIndex()
        {
            int randomIndex;
    
            do
            {
                randomIndex = Random.Range(0, gameModes.Count);
            } while (randomIndex == previousGameModeIndex);
    
            previousGameModeIndex = randomIndex;
            return randomIndex;
        }


        private void SwitchToGameMode(int index)
        {
            Map selectedMap = Main.Instance.Configuration.Instance.Maps.Find(map => map.MapID == currentMap);
            SetRandomTimeOfDay(selectedMap.TimeWeights);
            
            gameModes[index].Start();
        }
        
        private void SetRandomTimeOfDay(List<Time> timeWeights)
        {
            float totalWeight = timeWeights.Sum(time => time.Dawn + time.Day + time.Dusk + time.Night);
            float randomValue = Random.Range(0f, totalWeight);

            foreach (var time in timeWeights)
            {
                if (randomValue < time.Dawn)
                {
                    SetTimeOfDay(13000); // Dawn time
                    break;
                }
                if (randomValue < time.Dawn + time.Day)
                {
                    SetTimeOfDay(5000); // Day time
                    break;
                }
                if (randomValue < time.Dawn + time.Day + time.Dusk)
                {
                    SetTimeOfDay(17000); // Dusk time
                    break;
                }
                SetTimeOfDay(24000); // Night time
                break;
            }
        }

        private void SetTimeOfDay(int time)
        {
            R.Commands.Execute(new ConsolePlayer(), $"/time {time}");
        }
        
        public bool ChangeGameMode(UnturnedPlayer player, string newMode)
        {
            int newModeIndex = gameModes.FindIndex(mode => mode.Name.Equals(newMode, StringComparison.OrdinalIgnoreCase));

            if (newModeIndex == -1)
            {
                return false; // Invalid gamemode
            }
    
            gameModes[currentGameModeIndex].Stop();
            
            ReturnToLobby();
            SwitchToGameMode(newModeIndex);
    
            return true;
        }

        public TimeSpan GetRemainingTime()
        {
            if (currentGameModeIndex >= 0 && currentGameModeIndex < gameModes.Count)
            {
                return gameModes[currentGameModeIndex].RemainingTime;
            }
            else
            {
                // Handle the case where the currentGameModeIndex is invalid
                return TimeSpan.Zero; // Or some default value
            }
        }

        public void PlayerJoinedGame(UnturnedPlayer player)
        {
            if (!ActivePlayers.Contains(player))
            {
                ActivePlayers.Add(player);
                Logger.Log($"{player} joined game");
                
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    Embed embed = new Embed(null, $"**{player.SteamName}** deployed to the game", null, "16714764", DateTime.UtcNow.ToString("s"),
                        new Footer(Provider.serverName, Provider.configData.Browser.Icon),
                        new Author(player.SteamName, $"https://steamcommunity.com/profiles/{player.CSteamID}/", player.SteamProfile.AvatarIcon.ToString()),
                        new Field[]
                        {
                        },
                        null, null);
                    DiscordManager.SendEmbed(embed, "Deployed to game", Main.Instance.Configuration.Instance.DeployWebhook);
                });
            }
        }

        public void PlayerLeftGame(UnturnedPlayer player)
        {
            if (ActivePlayers.Contains(player))
            {
                ActivePlayers.Remove(player);
                Logger.Log($"{player} left game");
                
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    Embed embed = new Embed(null, $"**{player.SteamName}** returned to the lobby", null, "16714764", DateTime.UtcNow.ToString("s"),
                        new Footer(Provider.serverName, Provider.configData.Browser.Icon),
                        new Author(player.SteamName, $"https://steamcommunity.com/profiles/{player.CSteamID}/", player.SteamProfile.AvatarIcon.ToString()),
                        new Field[]
                        {
                        },
                        null, null);
                    DiscordManager.SendEmbed(embed, "Returned to lobby", Main.Instance.Configuration.Instance.DeployWebhook);
                });
            }
        }

        public IEnumerable<UnturnedPlayer> GetUnturnedPlayers() => ActivePlayers.ToList();
    }

    public class GameMode
    {
        public string Name { get; }
        public bool IsFinished { get; private set; }
        private TimeSpan remainingTime;
        public TimeSpan RemainingTime => remainingTime;
        private TimeSpan initialDuration;

        public GameMode(string name, TimeSpan duration)
        {
            Name = name;
            initialDuration = duration; // Store the initial duration
            remainingTime = duration;
        }

        public void Start()
        {
            remainingTime = initialDuration;
            IsFinished = false;
            
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Map selectedMap = Main.Instance.Configuration.Instance.Maps.Find(map => map.MapID == GameManager.Instance.currentMap);
                
                Embed embed = new Embed(null, $"A **{GameManager.Instance.gameModes[GameManager.Instance.currentGameModeIndex].Name}** game has started!", null, "16714764", DateTime.UtcNow.ToString("s"),
                    new Footer(Provider.serverName, Provider.configData.Browser.Icon),
                    new Author(null, null, null),
                    new Field[]
                    {
                        new Field("**Gamemode:**", $"{GameManager.Instance.gameModes[GameManager.Instance.currentGameModeIndex].Name}", true),
                        new Field("**Map:**", $"{selectedMap.MapName}", true),
                        new Field("**Duration:**", $"{GameManager.Instance.gameModes[GameManager.Instance.currentGameModeIndex].initialDuration}", true)
                    },
                    null, null);
                DiscordManager.SendEmbed(embed, "Game Started", Main.Instance.Configuration.Instance.GameInfoWebhook);
            });

            // Start the update loop in a separate task
            Task.Run(async () =>
            {
                while (!IsFinished && remainingTime > TimeSpan.Zero)
                {
                    TaskDispatcher.QueueOnMainThread(LogCountdown);
                    await Task.Delay(TimeSpan.FromSeconds(1)); // Log every second
                }
            });
        }

        public void Stop()
        {
            IsFinished = true;
        }
        
        public void Update(TimeSpan elapsedTime)
        {
            remainingTime -= elapsedTime;

            if (remainingTime <= TimeSpan.Zero)
            {
                IsFinished = true;
            }
        }
    
        private void LogCountdown()
        {
            TimeSpan formattedTime = TimeSpan.FromSeconds(Math.Ceiling(remainingTime.TotalSeconds));
            double remainingSeconds = formattedTime.TotalSeconds;
            
            if (Main.Instance.Configuration.Instance.LoggingEnabled)
            {
                Logger.Log($"Time remaining for {Name} mode: {formattedTime.TotalSeconds}");
            }

            if (remainingSeconds is >= 1 and <= 30)
            {
                List<int> countdownThresholds = new List<int> { 30, 10, 5, 4, 3, 2, 1 };
                if (countdownThresholds.Contains((int)remainingSeconds))
                {
                    Utility.Broadcast($"Only {(int)remainingSeconds} seconds left in this {Name} game!");
                }
            }
        }
    }
}
