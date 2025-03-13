using Microsoft.Data.Sqlite;
using System;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

class Database
{
    private static string _connectionString = "Data Source=botdata.db;Version=3;";
    private static readonly byte[] key = Encoding.UTF8.GetBytes("YourSecretKey123"); // Change this!

    public static void SaveUser(string discordToken, string twitchToken, string twitchUsername)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            string insertQuery = "INSERT INTO users (discord_token, twitch_token, twitch_username) VALUES (@discord, @twitch, @username);";
            using (var command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@discord", Encrypt(discordToken));
                command.Parameters.AddWithValue("@twitch", Encrypt(twitchToken));
                command.Parameters.AddWithValue("@username", twitchUsername);
                command.ExecuteNonQuery();
            }
        }
    }

    public static (string discordToken, string twitchToken, string twitchUsername)? GetUserById(int userId)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            string selectQuery = "SELECT discord_token, twitch_token, twitch_username FROM users WHERE id = @id;";
            using (var command = new SQLiteCommand(selectQuery, connection))
            {
                command.Parameters.AddWithValue("@id", userId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return (Decrypt(reader.GetString(0)), Decrypt(reader.GetString(1)), reader.GetString(2));
                    }
                }
            }
        }
        return null;
    }

    private static string Encrypt(string text)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
            {
                byte[] encrypted = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(text), 0, text.Length);
                byte[] result = new byte[iv.Length + encrypted.Length];
                Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);
                return Convert.ToBase64String(result);
            }
        }
    }

    private static string Decrypt(string encryptedText)
    {
        byte[] buffer = Convert.FromBase64String(encryptedText);
        using (var aes = Aes.Create())
        {
            aes.Key = key;
            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] cipherText = new byte[buffer.Length - iv.Length];
            Buffer.BlockCopy(buffer, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(buffer, iv.Length, cipherText, 0, cipherText.Length);

            using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
            {
                return Encoding.UTF8.GetString(decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length));
            }
        }
    }
}
