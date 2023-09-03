using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KevunsGameManager.Models;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using Steamworks;

namespace KevunsGameManager.Managers
{
    public class DatabaseManager
    {
        private string ConnectionString { get; set; }
        public List<GamePlayer> Data { get; set; }
        private List<PlayerStats> StatsData { get; set; }

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
            using MySqlConnection conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();
                await new MySqlCommand("CREATE TABLE IF NOT EXISTS `GamePlayerInfo` (`SteamID` BIGINT NOT NULL , `Username` VARCHAR(255) NOT NULL , `First Joined` DATETIME NOT NULL , `Last Joined` DATETIME NOT NULL , PRIMARY KEY (`SteamID`));", conn).ExecuteScalarAsync();
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
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    MySqlCommand comm = new MySqlCommand($"INSERT IGNORE INTO `GamePlayerInfo` (`SteamID`, `Username`, `First Joined`, `Last Joined`) VALUES ({steamID}, '{username}', @date, @date);", conn);
                    comm.Parameters.AddWithValue("@date", DateTime.UtcNow);
                    await comm.ExecuteScalarAsync();

                    lock (Data)
                    {
                        if (!Data.Exists(k => k.SteamID == steamID))
                        {
                            Data.Add(new GamePlayer(steamID, username, DateTime.UtcNow, DateTime.UtcNow));
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
            
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    MySqlCommand comm = new MySqlCommand($"INSERT IGNORE INTO `GamePlayerStats` (`SteamID`, `Username`, `Kills`, `Deaths`, `KDR`, `Headshots`, `Headshot Accuracy`) VALUES ({steamID}, '{username}', 0, 0, 1.0, 0, 1.0);", conn);
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

        public async Task SetColumnValueAsync(CSteamID steamID, int id, string columnName)
        {
            using MySqlConnection conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();
                await new MySqlCommand($"UPDATE `GamePlayerInfo` SET `{columnName}` = {id} WHERE `SteamID` = {steamID};", conn).ExecuteScalarAsync();

                lock (Data)
                {
                    GamePlayer data = Data.FirstOrDefault(k => k.SteamID == steamID);
                    if (data != null)
                    {
                        data.UpdateValue(columnName, id);
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

        public void ChangeLastJoin(CSteamID steamID, DateTime time)
        {
            Task.Run(async () => await SetDateTimeAsync(steamID, time, "Last Joined"));
        }

        private async Task SetDateTimeAsync(CSteamID steamID, DateTime dateTime, string columnName)
        {
            using MySqlConnection conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();
                var comm = new MySqlCommand($"UPDATE `GamePlayerInfo` SET `{columnName}` = @date WHERE `SteamID` = {steamID};", conn);
                comm.Parameters.AddWithValue("@date", dateTime);
                await comm.ExecuteScalarAsync();

                lock (Data)
                {
                    GamePlayer data = Data.FirstOrDefault(k => k.SteamID == steamID);
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
        
        public async Task<PlayerStats> GetPlayerStatsAsync(CSteamID steamID)
        {
            using var conn = new MySqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();

                // Query the database to get player stats based on the player's SteamID
                const string query = "SELECT * FROM `GamePlayerStats` WHERE `SteamID` = @steamID;";
                var comm = new MySqlCommand(query, conn);
                comm.Parameters.AddWithValue("@steamID", steamID.ToString()); // Convert SteamID to string

                using var reader = await comm.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // Parse the retrieved data and create a PlayerStats object
                    var username = reader["Username"].ToString(); // You can retrieve the username if needed
                    var kills = Convert.ToInt32(reader["Kills"]);
                    var deaths = Convert.ToInt32(reader["Deaths"]);
                    var kdr = Convert.ToDouble(reader["KDR"]);
                    var headshots = Convert.ToInt32(reader["Headshots"]);
                    var headshotAccuracy = Convert.ToDouble(reader["Headshot Accuracy"]);

                    var playerStats = new PlayerStats(steamID, username, kills, deaths, kdr, headshots, headshotAccuracy);
                    return playerStats;
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
                MySqlDataReader rdr = new MySqlCommand("SELECT * FROM `GamePlayerInfo`;", conn).ExecuteReader();

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

                            string username = rdr[1].ToString();

                            if (!DateTime.TryParse(rdr[2].ToString(), out DateTime firstJoined))
                            {
                                continue;
                            }

                            if (!DateTime.TryParse(rdr[3].ToString(), out DateTime lastJoined))
                            {
                                continue;
                            }

                            Data.Add(new GamePlayer(new CSteamID(id), username, firstJoined, lastJoined));
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
