using System;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rocket.Core.Utils;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace KevunsGameManager.Managers
{
    public class GameManager
    {
        private static GameManager instance;
        public static GameManager Instance => instance ??= new GameManager();

        private List<GameMode> gameModes;
        private int currentGameModeIndex;
        private const int GameModeLoopIncrement = 1;

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
            SwitchToGameMode(currentGameModeIndex);
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
        
        public void Update(TimeSpan elapsedTime)
        {
            gameModes[currentGameModeIndex].Update(elapsedTime);

            if (gameModes[currentGameModeIndex].IsFinished)
            {
                TaskDispatcher.QueueOnMainThread(() =>
                    {
                        ReturnToLobby();
                        try
                        {
                            Utility.Broadcast("A new game is starting!");
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(ex);
                        }

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
                    // return to lobby message
                });
                
                ActivePlayers.Remove(player);
            }
        }

        private void SwitchToNextGameMode()
        {
            currentGameModeIndex = (currentGameModeIndex + GameModeLoopIncrement) % gameModes.Count;
            SwitchToGameMode(currentGameModeIndex);
        }

        private void SwitchToGameMode(int index)
        {
            gameModes[index].Start();
        }
        
        public bool ChangeGameMode(UnturnedPlayer player, string newMode)
        {
            int newModeIndex = gameModes.FindIndex(mode => mode.Name.Equals(newMode, StringComparison.OrdinalIgnoreCase));

            if (newModeIndex == -1)
            {
                return false; // Invalid gamemode
            }
    
            // Stop the current game mode immediately by setting its remaining time to 0
            gameModes[currentGameModeIndex].Stop();

            ReturnToLobby();
            SwitchToGameMode(newModeIndex);
    
            return true;
        }



        public void PlayerJoinedGame(UnturnedPlayer player)
        {
            if (!ActivePlayers.Contains(player))
            {
                ActivePlayers.Add(player);
                Logger.Log($"{player} joined game");
            }
        }

        public void PlayerLeftGame(UnturnedPlayer player)
        {
            if (ActivePlayers.Contains(player))
            {
                ActivePlayers.Remove(player);
                Logger.Log($"{player} left game");
            }
        }

        public IEnumerable<UnturnedPlayer> GetUnturnedPlayers() => ActivePlayers.ToList();
    }

    public class GameMode
    {
        public string Name { get; }
        public TimeSpan Duration { get; }
        public bool IsFinished { get; private set; }

        private TimeSpan remainingTime;
        
        public TimeSpan RemainingTime => remainingTime;

        private TimeSpan initialDuration;

        public GameMode(string name, TimeSpan duration)
        {
            Name = name;
            initialDuration = duration; // Store the initial duration
            Duration = duration;
            remainingTime = duration;
        }

        public void Start()
        {
            remainingTime = initialDuration;
            IsFinished = false;

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

            if (remainingSeconds >= 1 && remainingSeconds <= 30)
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
