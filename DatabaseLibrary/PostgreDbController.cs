using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLibrary
{
    public class PostgreDbController: IDbController
    {
        public static string userId;
        public static string password;
        public static string databaseName;
        public static string port;
        public static string server;

        static IDbController internalController;

        string connectionString;

        public static IDbController Controller
        {
            get
            {
                if (internalController == null)
                {
                    lock("PostgreDbController lock string")
                    {
                        if (internalController == null)
                        {
                            internalController = new PostgreDbController();
                        }
                    }
                }
                return internalController;
            }
        }

        public Task SetDefaultCityAsync(BotLibrary.Conversation conversation, string cityName)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Insert or update data
                using (var sqlCommand = new NpgsqlCommand())
                {
                    sqlCommand.Connection = conn;
                    try
                    {
                        sqlCommand.CommandText = "INSERT INTO public.default_cities(platform, id, channel, city) VALUES (@platform, @id, @channel, @city)";
                        sqlCommand.Parameters.AddWithValue("platform", conversation.Platfrom);
                        sqlCommand.Parameters.AddWithValue("id", conversation.Id);
                        sqlCommand.Parameters.AddWithValue("channel", conversation.Channel);
                        sqlCommand.Parameters.AddWithValue("city", cityName);
                        sqlCommand.ExecuteNonQuery();
                    }
                    catch
                    {
                        sqlCommand.Parameters.Clear();
                        sqlCommand.CommandText = "UPDATE public.default_cities SET city=@city WHERE (platform = @platform AND id = @id AND channel = @channel)";
                        sqlCommand.Parameters.AddWithValue("platform", conversation.Platfrom);
                        sqlCommand.Parameters.AddWithValue("id", conversation.Id);
                        sqlCommand.Parameters.AddWithValue("channel", conversation.Channel);
                        sqlCommand.Parameters.AddWithValue("city", cityName);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            return Task.CompletedTask;
        }

        public async Task<string> GetDefaultCityAsync(BotLibrary.Conversation conversation)
        {
            string result = null;
            using (var conn = new NpgsqlConnection(connectionString))
            {                
                conn.Open();

                // Select some data
                using (var cmd = new NpgsqlCommand())
                {                    
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT city FROM public.default_cities WHERE (platform = @platform AND id = @id AND channel = @channel)";
                    cmd.Parameters.AddWithValue("platform", conversation.Platfrom);
                    cmd.Parameters.AddWithValue("id", conversation.Id);
                    cmd.Parameters.AddWithValue("channel", conversation.Channel);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = reader.GetString(0);
                        }                            
                    }
                }
            }
            return result;
        }

        private PostgreDbController()
        {
            connectionString = $"Server={server}; Port={port}; Database={databaseName}; Userid={userId}; Password={password}; SslMode=Require; Trust Server Certificate=true";
        }           
    }
}
