using System;
using System.Collections.Generic;

namespace FateCalculator.Models
{
    /// <summary>
    /// Таблицы соответствия ранга параметра и итогового показателя (HP/Мана/Атака).
    /// Ранг вида "B+" разбивается на базовый ранг "B" и уровень "+" (0 = нет, 1 = +, 2 = ++)
    /// </summary>
    public static class RankTables
    {
        private static readonly string[] BaseRanks = { "E", "D", "C", "B", "A", "EX" };

        private static readonly Dictionary<string, int[]> HpTable = new()
        {
            ["E"] = new[] { 175, 195, 210 },
            ["D"] = new[] { 250, 270, 290 },
            ["C"] = new[] { 300, 320, 340 },
            ["B"] = new[] { 400, 420, 440 },
            ["A"] = new[] { 450, 470, 490 },
            ["EX"] = new[] { 500, 520, 540 },
        };

        private static readonly Dictionary<string, int[]> ManaTable = new()
        {
            ["E"] = new[] { 70, 90, 105 },
            ["D"] = new[] { 105, 125, 155 },
            ["C"] = new[] { 175, 195, 225 },
            ["B"] = new[] { 250, 270, 300 },
            ["A"] = new[] { 350, 370, 400 },
            ["EX"] = new[] { 450, 470, 490 },
        };

        private static readonly Dictionary<string, int[]> AtkTable = new()
        {
            ["E"] = new[] { 20, 22, 24 },
            ["D"] = new[] { 25, 27, 29 },
            ["C"] = new[] { 30, 32, 34 },
            ["B"] = new[] { 35, 37, 39 },
            ["A"] = new[] { 40, 42, 44 },
            ["EX"] = new[] { 45, 47, 50 },
        };

        public static (string baseRank, int level) Parse(string rank)
        {
            if (string.IsNullOrWhiteSpace(rank)) return ("E", 0);
            int level = 0;
            string baseRank = rank;
            if (rank.EndsWith("++")) { level = 2; baseRank = rank.Substring(0, rank.Length - 2); }
            else if (rank.EndsWith("+")) { level = 1; baseRank = rank.Substring(0, rank.Length - 1); }
            return (baseRank, level);
        }

        public static int GetHp(string rank)
        {
            var (b, l) = Parse(rank);
            return HpTable.TryGetValue(b, out var arr) ? arr[l] : HpTable["E"][0];
        }

        public static int GetMana(string rank)
        {
            var (b, l) = Parse(rank);
            return ManaTable.TryGetValue(b, out var arr) ? arr[l] : ManaTable["E"][0];
        }

        public static int GetAttack(string rank)
        {
            var (b, l) = Parse(rank);
            return AtkTable.TryGetValue(b, out var arr) ? arr[l] : AtkTable["E"][0];
        }
    }
}
