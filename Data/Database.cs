using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using FateCalculator.Models;

namespace FateCalculator.Data
{
    public static class Database
    {
        private static string DbPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fate_calculator.db");

        private static string ConnectionString => $"Data Source={DbPath}";

        public static void Initialize()
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Characters (
                    Id TEXT PRIMARY KEY,
                    Name TEXT,
                    Json TEXT
                );";
            cmd.ExecuteNonQuery();
        }

        public static List<Character> LoadAll()
        {
            var result = new List<Character>();
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Json FROM Characters;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var json = reader.GetString(0);
                try
                {
                    var ch = JsonSerializer.Deserialize<Character>(json);
                    if (ch != null) result.Add(ch);
                }
                catch { /* пропускаем повреждённую запись */ }
            }
            return result;
        }

        public static void Save(Character character)
        {
            var json = JsonSerializer.Serialize(character);
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Characters (Id, Name, Json) VALUES ($id, $name, $json)
                ON CONFLICT(Id) DO UPDATE SET Name = $name, Json = $json;";
            cmd.Parameters.AddWithValue("$id", character.Id);
            cmd.Parameters.AddWithValue("$name", character.Name);
            cmd.Parameters.AddWithValue("$json", json);
            cmd.ExecuteNonQuery();
        }

        public static void Delete(string id)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Characters WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
