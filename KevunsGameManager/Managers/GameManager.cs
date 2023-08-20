using Rocket.Core.Steam;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using System.Linq;
using System;
using Rocket.Core.Logging;

namespace KevunsGameManager.Managers
{
    public class GameManager
    {
        private static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameManager();
                }
                return instance;
            }
        }

        private List<GameMode> gameModes;
        private int currentGameModeIndex = 0;

        public List<UnturnedPlayer> ActivePlayers { get; private set; } = new List<UnturnedPlayer>();

        public GameManager()
        {
            ActivePlayers = new List<UnturnedPlayer>();
            gameModes = new List<GameMode>
            {
                new GameMode("FFA", TimeSpan.FromMinutes(20)),
                new GameMode("TDM", TimeSpan.FromMinutes(20))
            };
        }

        public void Start()
        {
            SwitchToGameMode(currentGameModeIndex);
        }

        public void Update(TimeSpan elapsedTime)
        {
            gameModes[currentGameModeIndex].Update(elapsedTime);

            if (gameModes[currentGameModeIndex].IsFinished)
            {
                SwitchToNextGameMode();
            }
        }

        private void SwitchToNextGameMode()
        {
            currentGameModeIndex = (currentGameModeIndex + 1) % gameModes.Count;
            SwitchToGameMode(currentGameModeIndex);
        }

        private void SwitchToGameMode(int index)
        {
            GameMode newGameMode = gameModes[index];
            newGameMode.Start();
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

        public IEnumerable<UnturnedPlayer> GetUnturnedPlayers()
        {
            return ActivePlayers.ToList();
        }
    }

    public class GameMode
    {
        public string Name { get; }
        public TimeSpan Duration { get; }
        public bool IsFinished { get; private set; }

        private TimeSpan remainingTime;

        public GameMode(string name, TimeSpan duration)
        {
            Name = name;
            Duration = duration;
            remainingTime = duration;
        }

        public void Start()
        {
            remainingTime = Duration;
            IsFinished = false;
        }

        public void Update(TimeSpan elapsedTime)
        {
            remainingTime -= elapsedTime;

            if (remainingTime <= TimeSpan.Zero)
            {
                IsFinished = true;
            }
        }
    }
}