using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConquestGameManager.Models;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using Steamworks;

namespace ConquestGameManager.Managers
{
    public class DatabaseManager
    {
        private static string ConnectionString { get; set; }
        public List<GamePlayer> Data { get; set; }
        private List<PlayerStats> StatsData { get; set; }
        public readonly Dictionary<CSteamID, int> PlaytimeTracker = new Dictionary<CSteamID, int>();


        public DatabaseManager() 
        { 
            ConnectionString = Main.Instance.Configuration.Instance.ConnectionString;
            Data = new List<GamePlayer>();
            StatsData = new List<PlayerStats>();

            Task.Run(async () =>
            {
                await InitiateDatabaseAsync();
                CacheGet();
            });
        }

        private async Task InitiateDatabaseAsync()
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();
                await new MySqlCommand("CREATE TABLE IF NOT EXISTS `GamePlayerInfo` (`SteamID` BIGINT NOT NULL , `Username` VARCHAR(255) NOT NULL , `Rank` INT NOT NULL , `XP` INT NOT NULL , `Health Level` INT NOT NULL , `Movement Level` INT NOT NULL , `Jump Level` INT NOT NULL , `Stamina Level` INT NOT NULL , `First Joined` DATETIME NOT NULL , `Last Joined` DATETIME NOT NULL , `Playtime` INT NOT NULL , PRIMARY KEY (`SteamID`));", conn).ExecuteScalarAsync();
                await new MySqlCommand("CREATE TABLE IF NOT EXISTS `GamePlayerStats` (`SteamID` BIGINT NOT NULL , `Username` VARCHAR(255) NOT NULL , `Kills` INT NOT NULL , `Deaths` INT NOT NULL , `KDR` DOUBLE NOT NULL , `Headshots` INT NOT NULL , `Headshot Accuracy` DOUBLE NOT NULL , PRIMARY KEY (`SteamID`));", conn).ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                Logger.Log("Error during database initialisation!");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task AddPlayerAsync(CSteamID steamID, string username)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    var comm = new MySqlCommand($"INSERT IGNORE INTO `GamePlayerInfo` (`SteamID`, `Username`, `Rank`, `XP`, `Health Level`, `Movement Level`, `Jump Level`, `Stamina Level`, `First Joined`, `Last Joined`, `Playtime`) VALUES (@steamID, @username, 0, 0, 0, 0, 0, 0, @date, @date, 0);", conn);
                    comm.Parameters.AddWithValue("@date", DateTime.UtcNow);
                    comm.Parameters.AddWithValue("@steamID", steamID);
                    comm.Parameters.AddWithValue("@username", username);
                    await comm.ExecuteScalarAsync();

                    lock (Data)
                    {
                        if (!Data.Exists(k => k.SteamID == steamID))
                        {
                            Data.Add(new GamePlayer(steamID, username, 0, 0, 0, 0, 0, 0, DateTime.UtcNow, DateTime.UtcNow));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Error adding player to database!");
                    Logger.Log(ex);
                }
                finally
                {
                    await conn.CloseAsync();
                }
            }
            
            using (var conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    var comm = new MySqlCommand($"INSERT IGNORE INTO `GamePlayerStats` (`SteamID`, `Username`, `Kills`, `Deaths`, `KDR`, `Headshots`, `Headshot Accuracy`) VALUES ({steamID}, '{username}', 0, 0, 1.0, 0, 1.0);", conn);
                    await comm.ExecuteScalarAsync();

                    lock (StatsData)
                    {
                        if (!StatsData.Exists(k => k.SteamID == steamID))
                        {
                            StatsData.Add(new PlayerStats(steamID, username, 0, 0, 1.0, 0, 1.0));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Error adding player to database!");
                    Logger.Log(ex);
                }
                finally
                {
                    await conn.CloseAsync();
                }
            }
        }

        private async Task SetColumnValueAsync(CSteamID steamID, int id, string columnName)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();
                await new MySqlCommand($"UPDATE `GamePlayerInfo` SET `{columnName}` = {id} WHERE `SteamID` = {steamID};", conn).ExecuteScalarAsync();

                lock (Data)
                {
                    GamePlayer data = Data.FirstOrDefault(k => k.SteamID == steamID);
                    if (data != null)
                    {
                        GamePlayer.UpdateValue(columnName, id);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error changing values of {steamID} for column {columnName} to {id}");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public void StartPlaytimeTracking(CSteamID steamID)
        {
            if (!PlaytimeTracker.ContainsKey(steamID))
            {
                PlaytimeTracker.Add(steamID, 0);
            }

            var timer = new Timer(Callback, null, 1000, 1000);
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            return;

            void Callback(object state)
            {
                if (PlaytimeTracker.ContainsKey(steamID))
                {
                    PlaytimeTracker[steamID]++;
                }
            }
        }

        public int GetCurrentPlaytime(CSteamID steamID)
        {
            return PlaytimeTracker.TryGetValue(steamID, out var currentPlaytime) ? currentPlaytime : 0;
        }

        public async Task UpdatePlaytimeAsync(CSteamID steamID, int playtimeSeconds)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();
                await new MySqlCommand($"UPDATE `GamePlayerInfo` SET `Playtime` = `Playtime` + {playtimeSeconds} WHERE `SteamID` = {steamID};", conn).ExecuteScalarAsync();

                lock (Data)
                {
                    var data = Data.FirstOrDefault(k => k.SteamID == steamID);
                    if (data != null)
                    {
                        data.Playtime += playtimeSeconds;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating playtime for {steamID} in the database!");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }
        
        public static async Task<int> GetPlaytimeAsync(CSteamID steamID)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();

                const string query = "SELECT `PlayTime` FROM `GamePlayerInfo` WHERE `SteamID` = @steamID;";
                var comm = new MySqlCommand(query, conn);
                comm.Parameters.AddWithValue("@steamID", steamID.ToString());

                var playtime = await comm.ExecuteScalarAsync();
                if (playtime != null && int.TryParse(playtime.ToString(), out int playtimeSeconds))
                {
                    return playtimeSeconds;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error fetching playtime for {steamID} from the database!");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }

            return 0;
        }
        
        public void ChangeLastJoin(CSteamID steamID, DateTime time)
        {
            Task.Run(async () => await SetDateTimeAsync(steamID, time, "Last Joined"));
        }

        private async Task SetDateTimeAsync(CSteamID steamID, DateTime dateTime, string columnName)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();
                var comm = new MySqlCommand($"UPDATE `GamePlayerInfo` SET `{columnName}` = @date WHERE `SteamID` = {steamID};", conn);
                comm.Parameters.AddWithValue("@date", dateTime);
                await comm.ExecuteScalarAsync();

                lock (Data)
                {
                    var data = Data.FirstOrDefault(k => k.SteamID == steamID);
                    data?.UpdateDateTime(columnName, dateTime);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error changing time of {steamID} for column {columnName} to {dateTime}");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        private async Task PromotePlayerAsync(CSteamID steamID, int nextRankID)
        {
            var player = await GetPlayerInfoAsync(steamID);

            if (player != null)
            {
                player.Rank = nextRankID;
                await SetColumnValueAsync(steamID, player.Rank, "Rank");
            }
        }
        
        public async Task UpdateDeathCountAsync(UnturnedPlayer player)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();

                const string query = "UPDATE `GamePlayerStats` SET `Deaths` = `Deaths` + 1, `KDR` = IF(`Deaths` = 0, `Kills`, `Kills` / `Deaths`) WHERE `SteamID` = @steamID;";
                var comm = new MySqlCommand(query, conn);
                comm.Parameters.AddWithValue("@steamID", player.CSteamID.ToString());
                await comm.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating death count for {player.DisplayName} in the database!");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task UpdateKillCountAsync(UnturnedPlayer player, bool wasHeadshot)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();

                var query = wasHeadshot
                    ? "UPDATE `GamePlayerStats` SET `Kills` = `Kills` + 1, `Headshots` = `Headshots` + 1, `KDR` = IF(`Deaths` = 0, `Kills`, `Kills` / `Deaths`), `Headshot Accuracy` = `Headshots` / `Kills` WHERE `SteamID` = @steamID;"
                    : "UPDATE `GamePlayerStats` SET `Kills` = `Kills` + 1, `KDR` = IF(`Deaths` = 0, `Kills`, `Kills` / `Deaths`), `Headshot Accuracy` = `Headshots` / `Kills` WHERE `SteamID` = @steamID;";

                var comm = new MySqlCommand(query, conn);
                comm.Parameters.AddWithValue("@steamID", player.CSteamID.ToString());
                await comm.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating kill count for {player.DisplayName} in the database!");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        private static async Task<int> GetXpAsync(GamePlayer gPlayer)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();

                const string query = "SELECT `XP` FROM `GamePlayerInfo` WHERE `SteamID` = @steamID;";
                var comm = new MySqlCommand(query, conn);
                comm.Parameters.AddWithValue("@steamID", gPlayer.SteamID);

                var xp = await comm.ExecuteScalarAsync();
                if (xp != null && int.TryParse(xp.ToString(), out int xpValue))
                {
                    return xpValue;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error fetching XP for {gPlayer.SteamID} from the database!");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }

            return 0;
        }

        
        public async Task UpdateXpAsync(CSteamID murderer, bool wasHeadshot)
        {
            var player = UnturnedPlayer.FromCSteamID(murderer);
            var uPlayer = Main.Instance.DatabaseManager.Data.FirstOrDefault(k => k.SteamID == player.CSteamID);
            if (uPlayer == null)
            {
                Logger.Log($"Error: GamePlayer not found for murderer CSteamID: {murderer}");
                return;
            }
            var xp = Main.Instance.Configuration.Instance.KillXp;
            if (wasHeadshot)
            {
                xp += Main.Instance.Configuration.Instance.HeadshotBonusXp;
            }
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();

                const string query = "UPDATE `gameplayerinfo` SET `XP` = `XP` + @xp WHERE `SteamID` = @steamID;";
                var comm = new MySqlCommand(query, conn);
                comm.Parameters.AddWithValue("@xp", xp);
                comm.Parameters.AddWithValue("@steamID", murderer);
                await comm.ExecuteNonQueryAsync();

                Logger.Log("Calling check for rank up");
                Logger.Log($"killerPlayerGamePlayer: {uPlayer}");
                await CheckAndHandleRankUp(uPlayer);
                Logger.Log("Rank up check called");

            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating XP & Rank for {murderer} in the database!");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        private static async Task CheckAndHandleRankUp(GamePlayer gPlayer)
        {
            var playerRank = await GetPlayerRankAsync(gPlayer.SteamID);
            var nextRankID = playerRank + 1;
            Logger.Log($"next rank id is: {nextRankID}");
            var nextRank = Main.Instance.Configuration.Instance.Ranks.FirstOrDefault(rank => rank.RankID == nextRankID);
            Logger.Log($"next rank name is: {nextRank}");
            var playerXp = await GetXpAsync(gPlayer);

            if (nextRank != null && playerXp >= nextRank.RequiredXp)
            {
                Logger.Log($"Promoting player {gPlayer.Username}");
                await Main.Instance.DatabaseManager.PromotePlayerAsync(gPlayer.SteamID, nextRankID);
                await Main.Instance.DatabaseManager.UpdateSkillLevelsAsync(gPlayer.SteamID);
            }
        }
        
        public async Task UpdatePlayerSkillLevelAsync(CSteamID steamID, int skillLevel, string skillType)
        {
            try
            {
                await UpdateSkillLevelAsync(steamID, skillLevel, skillType);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating {skillType} level for {steamID} in the database!");
                Logger.Log(ex);
            }
        }

        private async Task UpdateSkillLevelAsync(CSteamID steamID, int level, string skillType)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();
                await new MySqlCommand($"UPDATE `GamePlayerInfo` SET `{skillType}` = {level} WHERE `SteamID` = {steamID};", conn).ExecuteScalarAsync();

                lock (Data)
                {
                    GamePlayer data = Data.FirstOrDefault(k => k.SteamID == steamID);
                    if (data != null)
                    {
                        data.UpdateSkillLevel(skillType, level);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error changing {skillType} level of {steamID} to {level}");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }
        
        public async Task<GamePlayer> GetPlayerSkillLevelsAsync(CSteamID steamID)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();

                const string query = "SELECT `Health Level`, `Movement Level`, `Jump Level`, `Stamina Level` FROM `GamePlayerInfo` WHERE `SteamID` = @steamID;";
                var comm = new MySqlCommand(query, conn);
                comm.Parameters.AddWithValue("@steamID", steamID.ToString());

                using var reader = await comm.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var healthLevel = Convert.ToInt32(reader["Health Level"]);
                    var movementLevel = Convert.ToInt32(reader["Movement Level"]);
                    var jumpLevel = Convert.ToInt32(reader["Jump Level"]);
                    var staminaLevel = Convert.ToInt32(reader["Stamina Level"]);

                    return new GamePlayer(steamID, "", 0,0, healthLevel, movementLevel, jumpLevel, staminaLevel, DateTime.MinValue, DateTime.MinValue);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error fetching skill levels for {steamID} from the database!");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }

            return null;
        }
        
        public async Task UpdateSkillLevelsAsync(CSteamID steamID)
        {
            var skillType = "test";
            var skillLevel = 0;
            
            await UpdatePlayerSkillLevelAsync(steamID, skillLevel, skillType);
        }
        
        public async Task<GamePlayer> GetPlayerInfoAsync(CSteamID steamID)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();

                const string query = "SELECT * FROM `gameplayerinfo` WHERE `SteamID` = @steamID;";
                var comm = new MySqlCommand(query, conn);
                comm.Parameters.AddWithValue("@steamID", steamID.ToString());

                using var reader = await comm.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var username = reader["Username"].ToString();
                    var firstJoined = Convert.ToDateTime(reader["First Joined"]);
                    var lastJoined = Convert.ToDateTime(reader["Last Joined"]);

                    return new GamePlayer(steamID, username, 0, 0, 0, 0,0 ,0, firstJoined, lastJoined);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error fetching player info for {steamID} from the database!");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
            return null;
        }

        private static async Task<int> GetPlayerRankAsync(CSteamID steamID)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();

                const string query = "SELECT `Rank` FROM `GamePlayerInfo` WHERE `SteamID` = @steamID;";
                var comm = new MySqlCommand(query, conn);
                comm.Parameters.AddWithValue("@steamID", steamID.ToString());

                var rank = await comm.ExecuteScalarAsync();
                if (rank != null && int.TryParse(rank.ToString(), out int rankValue))
                {
                    return rankValue;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error fetching rank for {steamID} from the database!");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }

            return 0;
        }

        public async Task<PlayerStats> GetPlayerStatsAsync(CSteamID steamID)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();

                const string query = "SELECT * FROM `gameplayerstats` WHERE `SteamID` = @steamID;";
                var comm = new MySqlCommand(query, conn);
                comm.Parameters.AddWithValue("@steamID", steamID.ToString());

                using var reader = await comm.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var username = reader["Username"].ToString();
                    var kills = Convert.ToInt32(reader["Kills"]);
                    var deaths = Convert.ToInt32(reader["Deaths"]);
                    var kdr = Convert.ToDouble(reader["KDR"]);
                    var headshots = Convert.ToInt32(reader["Headshots"]);
                    var headshotAccuracy = Convert.ToDouble(reader["Headshot Accuracy"]);

                    return new PlayerStats(steamID, username, kills, deaths, kdr, headshots, headshotAccuracy);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error fetching player stats for {steamID} from the database!");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
            return null;
        }

        public async Task<List<PlayerStats>> GetTopPlayersByKillsAsync(int playersPerPage, int startIndex)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();
                const string query = "SELECT * FROM `GamePlayerStats` ORDER BY `Kills` DESC LIMIT @startIndex, @playersPerPage;";
                var comm = new MySqlCommand(query, conn);
                comm.Parameters.AddWithValue("@startIndex", startIndex);
                comm.Parameters.AddWithValue("@playersPerPage", playersPerPage);

                using var reader = await comm.ExecuteReaderAsync();
                var topPlayers = new List<PlayerStats>();

                while (await reader.ReadAsync())
                {
                    var steamID = new CSteamID(Convert.ToUInt64(reader["SteamID"]));
                    var username = reader["Username"].ToString();
                    var kills = Convert.ToInt32(reader["Kills"]);

                    var playerStats = new PlayerStats(steamID, username, kills, 0, 0.0, 0, 0.0);
                    topPlayers.Add(playerStats);
                }
                return topPlayers;
            }
            catch (Exception ex)
            {
                Logger.Log("Error fetching top players by kills from the database!");
                Logger.Log(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
            return null;
        }
        
        private void CacheGet()
        {
            using MySqlConnection conn = new MySqlConnection(ConnectionString);
            try
            {
                conn.Open();
                var rdr = new MySqlCommand("SELECT * FROM `GamePlayerInfo`;", conn).ExecuteReader();

                try
                {
                    lock (Data)
                    {
                        Data.Clear();

                        while (rdr.Read())
                        {
                            if (!ulong.TryParse(rdr[0].ToString(), out ulong id))
                            {
                                continue;
                            }

                            var username = rdr[1].ToString();

                            if (!DateTime.TryParse(rdr[2].ToString(), out DateTime firstJoined))
                            {
                                continue;
                            }

                            if (!DateTime.TryParse(rdr[3].ToString(), out DateTime lastJoined))
                            {
                                continue;
                            }

                            Data.Add(new GamePlayer(new CSteamID(id), username, 0, 0, 0, 0,0 ,0, firstJoined, lastJoined));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Error while reading data!");
                    Logger.Log(ex);
                }
                finally
                {
                    rdr.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error during cache refresh!");
                Logger.Log(ex);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
