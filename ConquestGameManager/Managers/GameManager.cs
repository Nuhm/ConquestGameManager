using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConquestGameManager.Webhook;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Utils;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using Logger = Rocket.Core.Logging.Logger;
using Random = UnityEngine.Random;

namespace ConquestGameManager.Managers
{
    public class PlayerInfo
    {
        public CSteamID SteamID { get; }
        public string AssignedTeam { get; set; }

        public PlayerInfo(CSteamID steamId)
        {
            SteamID = steamId;
        }
    }
    public class GameManager
    {
        private static GameManager instance;
        public static GameManager Instance => instance ??= new GameManager();
        public List<GameMode> gameModes;
        public int currentGameModeIndex;
        public int currentMap;
        private int previousGameModeIndex = -1;
        private bool gameTransitionInProgress;
        public static List<UnturnedPlayer> AllPlayers { get; } = new();
        public static List<UnturnedPlayer> ActivePlayers { get; } = new();
        public static List<PlayerInfo> AllPlayersInfo { get; } = new List<PlayerInfo>();

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
            gameTransitionInProgress = false;
            Logger.Log("What's calling this? START");
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
            if (gameTransitionInProgress)
            {
                // You can either wait and retry or simply return without starting a new game
                return;
            }
            
            gameModes[currentGameModeIndex].Update(elapsedTime);

            if (!gameModes[currentGameModeIndex].IsFinished) return;
            ThreadPool.QueueUserWorkItem((o) =>
            {
                var embed = new Embed(null, $"A **{GameManager.Instance.gameModes[GameManager.Instance.currentGameModeIndex].Name}** game has ended!", null, "16714764", DateTime.UtcNow.ToString("s"),
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
                    if (gameTransitionInProgress)
                    {
                        // You can either wait and retry or simply return without starting a new game
                        return;
                    }
                    
                    var playersToTeleport = ActivePlayers.ToList();
                    foreach (var player in playersToTeleport)
                    {
                        SpawnManager.Instance.ReturnToLobby(player);
                    }
                        
                    Utility.Broadcast("The game has just ended!");
                    Utility.Broadcast("A new game is starting!");

                    Logger.Log("What's calling this? UPDATE");
                    SwitchToNextGameMode();
                }
            );
        }

        private void SwitchToNextGameMode()
        {
            Logger.Log("What's calling this? FINAL");
            // Check if a game transition is already in progress
            if (gameTransitionInProgress)
            {
                // You can either wait and retry or simply return without starting a new game
                return;
            }

            var enabledMaps = Main.Instance.Configuration.Instance.Maps.Where(map => map.IsEnabled).ToList();
            if (enabledMaps.Count > 0)
            {
                var randomMapIndex = Random.Range(0, enabledMaps.Count);
                var randomMap = enabledMaps[randomMapIndex];
                currentMap = randomMap.MapID;
            }
            else
            {
                currentMap = 0;
            }

            var randomGameModeIndex = GetRandomNonRepeatingGameModeIndex();
            currentGameModeIndex = randomGameModeIndex;
            
            // Set the flag to indicate that a transition is in progress
            gameTransitionInProgress = true;

            // Delay the game mode switch until the current game has fully ended
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1)); // Adjust the delay as needed
                SwitchToGameMode(currentGameModeIndex);
                
                // End the game and wipe player lists
                EndGame();
            });
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
            var selectedMap = Main.Instance.Configuration.Instance.Maps.Find(map => map.MapID == currentMap);
            SetRandomTimeOfDay(selectedMap.TimeWeights);
            
            gameModes[index].Start();
            gameTransitionInProgress = false;
        }
        
        private void SetRandomTimeOfDay(List<Models.Time> timeWeights)
        {
            var totalWeight = timeWeights.Sum(time => time.Dawn + time.Day + time.Dusk + time.Night);
            var randomValue = Random.Range(0f, totalWeight);

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

        private static void SetTimeOfDay(int time)
        {
            TaskDispatcher.QueueOnMainThread(() =>
            {
                R.Commands.Execute(new ConsolePlayer(), $"/time {time}");
            });
        }

        
        public bool ChangeGameMode(string newMode)
        {
            Logger.Log("STARTUNGNNGNGNGGN");
            // Check if a game transition is already in progress
            if (gameTransitionInProgress)
            {
                // You can either wait and retry or return false
                return false;
            }

            // Set the flag to indicate that a transition is in progress
            gameTransitionInProgress = true;

            gameModes[currentGameModeIndex].Stop();

            // Add a 1-second delay if needed

            var newModeIndex = gameModes.FindIndex(mode => mode.Name.Equals(newMode, StringComparison.OrdinalIgnoreCase));

            if (newModeIndex == -1 || newModeIndex == currentGameModeIndex)
            {
                // Reset the flag since no transition occurred
                gameTransitionInProgress = false;
                return false; // Invalid game mode or same game mode, no change needed
            }

            var playersToTeleport = ActivePlayers.ToList();
            foreach (var player in playersToTeleport)
            {
                SpawnManager.Instance.ReturnToLobby(player);
            }
            Logger.Log("RUNNNNNN");
            SwitchToGameMode(newModeIndex);

            // Reset the flag now that the transition is complete
            gameTransitionInProgress = false;

            Logger.Log("ENDDDD");
            return true;
        }

        public TimeSpan GetRemainingTime()
        {
            if (currentGameModeIndex >= 0 && currentGameModeIndex < gameModes.Count)
            {
                return gameModes[currentGameModeIndex].RemainingTime;
            }
            return TimeSpan.Zero;
        }

        public void PlayerJoinedGame(UnturnedPlayer player)
        {
            if (ActivePlayers.Contains(player)) return;
    
            AllPlayersInfo.Add(new PlayerInfo(player.CSteamID));
            ActivePlayers.Add(player);
            
            Logger.Log($"{player} joined game");
                
            ThreadPool.QueueUserWorkItem((o) =>
            {
                var embed = new Embed(null, $"**{player.SteamName}** deployed to the game", null, "16714764", DateTime.UtcNow.ToString("s"),
                    new Footer(Provider.serverName, Provider.configData.Browser.Icon),
                    new Author(player.SteamName, $"https://steamcommunity.com/profiles/{player.CSteamID}/", player.SteamProfile.AvatarIcon.ToString()),
                    new Field[]
                    {
                    },
                    null, null);
                DiscordManager.SendEmbed(embed, "Deployed to game", Main.Instance.Configuration.Instance.DeployWebhook);
            });
        }

        public void PlayerLeftGame(UnturnedPlayer player)
        {
            ActivePlayers.Remove(player);
            
            Logger.Log($"{player} left game");
                
            ThreadPool.QueueUserWorkItem((o) =>
            {
                var embed = new Embed(null, $"**{player.SteamName}** returned to the lobby", null, "16714764", DateTime.UtcNow.ToString("s"),
                    new Footer(Provider.serverName, Provider.configData.Browser.Icon),
                    new Author(player.SteamName, $"https://steamcommunity.com/profiles/{player.CSteamID}/", player.SteamProfile.AvatarIcon.ToString()),
                    new Field[]
                    {
                    },
                    null, null);
                DiscordManager.SendEmbed(embed, "Returned to lobby", Main.Instance.Configuration.Instance.DeployWebhook);
            });
        }
        
        private void EndGame()
        {
            AllPlayersInfo.Clear();
            AllPlayers.Clear();
            ActivePlayers.Clear();
        }
        
        public bool IsPlayerAssignedToTeam(UnturnedPlayer player)
        {
            return AllPlayersInfo.Any(p => p.SteamID == player.CSteamID && !string.IsNullOrEmpty(p.AssignedTeam));
        }

        public void SetPlayerTeam(UnturnedPlayer player, string teamName)
        {
            var existingPlayer = AllPlayersInfo.FirstOrDefault(p => p.SteamID == player.CSteamID);

            if (existingPlayer != null)
            {
                existingPlayer.AssignedTeam = teamName;
            }
        }

        public string GetPlayerTeam(UnturnedPlayer player)
        {
            var existingPlayer = AllPlayersInfo.FirstOrDefault(p => p.SteamID == player.CSteamID);
            return existingPlayer?.AssignedTeam;
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
                var selectedMap = Main.Instance.Configuration.Instance.Maps.Find(map => map.MapID == GameManager.Instance.currentMap);
                
                var embed = new Embed(null, $"A **{GameManager.Instance.gameModes[GameManager.Instance.currentGameModeIndex].Name}** game has started!", null, "16714764", DateTime.UtcNow.ToString("s"),
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
            remainingTime = TimeSpan.Zero;
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
            var formattedTime = TimeSpan.FromSeconds(Math.Ceiling(remainingTime.TotalSeconds));
            var remainingSeconds = formattedTime.TotalSeconds;
            
            if (Main.Instance.Configuration.Instance.LoggingEnabled)
            {
                //Logger.Log($"Time remaining for {Name} mode: {formattedTime.TotalSeconds}");
            }

            if (remainingSeconds is < 1 or > 30) return;
            var countdownThresholds = new List<int> { 30, 10, 5, 4, 3, 2, 1 };
            if (countdownThresholds.Contains((int)remainingSeconds))
            {
                Utility.Broadcast($"Only {(int)remainingSeconds} seconds left in this {Name} game!");
            }
        }
    }
}
