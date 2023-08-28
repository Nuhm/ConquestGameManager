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
        public string ConnectionString { get; set; }
        public List<GamePlayer> Data { get; set; }

        public DatabaseManager() 
        { 
            ConnectionString = Main.Instance.Configuration.Instance.ConnectionString;
            Data = new List<GamePlayer>();

            Task.Run(async () =>
            {
                await InitiateDatabaseAsync();
                CacheGet();
            });
        }

        public async Task InitiateDatabaseAsync()
        {
            using (MySqlConnection Conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    await Conn.OpenAsync();
                    await new MySqlCommand("CREATE TABLE IF NOT EXISTS `GamePlayerInfo` (`SteamID` BIGINT NOT NULL , `Username` VARCHAR(255) NOT NULL , `First Joined` DATETIME NOT NULL , `Last Joined` DATETIME NOT NULL , PRIMARY KEY (`SteamID`));", Conn).ExecuteScalarAsync();
                }
                catch (Exception ex)
                {
                    Logger.Log("Error during database initilisation!");
                    Logger.Log(ex);
                }
                finally
                {
                    await Conn.CloseAsync();
                }
            }
        }

        public async Task AddPlayerAsync(CSteamID steamID, string username)
        {
            using (MySqlConnection Conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    await Conn.OpenAsync();
                    MySqlCommand Comm = new MySqlCommand($"INSERT IGNORE INTO `GamePlayerInfo` (`SteamID`, `Username`, `First Joined`, `Last Joined`) VALUES ({steamID}, '{username}', @date, @date);", Conn);
                    Comm.Parameters.AddWithValue("@date", DateTime.UtcNow);
                    await Comm.ExecuteScalarAsync();

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
                    await Conn.CloseAsync();
                }
            }
        }

        public async Task SetColoumnValueAsync(CSteamID steamID, int id, string coloumnName)
        {
            using (MySqlConnection Conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    await Conn.OpenAsync();
                    await new MySqlCommand($"UPDATE `GamePlayerInfo` SET `{coloumnName}` = {id} WHERE `SteamID` = {steamID};", Conn).ExecuteScalarAsync();

                    lock (Data)
                    {
                        GamePlayer data = Data.FirstOrDefault(k => k.SteamID == steamID);
                        if (data != null)
                        {
                            data.UpdateValue(coloumnName, id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error changing values of {steamID} for coloumn {coloumnName} to {id}");
                    Logger.Log(ex);
                }
                finally
                {
                    await Conn.CloseAsync();
                }
            }
        }

        public void ChangeLastJoin(CSteamID steamID, DateTime time)
        {
            Task.Run(async () => await SetDateTimeAsync(steamID, time, "Last Joined"));
        }

        public async Task SetDateTimeAsync(CSteamID steamID, DateTime dateTime, string coloumnName)
        {
            using (MySqlConnection Conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    await Conn.OpenAsync();
                    MySqlCommand comm = new MySqlCommand($"UPDATE `GamePlayerInfo` SET `{coloumnName}` = @date WHERE `SteamID` = {steamID};", Conn);
                    comm.Parameters.AddWithValue("@date", dateTime);
                    await comm.ExecuteScalarAsync();

                    lock (Data)
                    {
                        GamePlayer data = Data.FirstOrDefault(k => k.SteamID == steamID);
                        if (data != null)
                        {
                            data.UpdateDateTime(coloumnName, dateTime);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error changing time of {steamID} for coloumn {coloumnName} to {dateTime}");
                    Logger.Log(ex);
                }
                finally
                {
                    await Conn.CloseAsync();
                }
            }
        }

        public void CacheGet()
        {
            using (MySqlConnection Conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    Conn.Open();
                    MySqlDataReader rdr = new MySqlCommand("SELECT * FROM `GamePlayerInfo`;", Conn).ExecuteReader();

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
                    Conn.Close();
                }
            }
        }
    }
}
