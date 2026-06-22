using System;
using System.Collections.Generic;

namespace FateCalculator.Models
{
    public class Character
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Безымянный";
        public string ClassType { get; set; } = "Сейбер";
        public string ImagePath { get; set; } = "";

        public double HeightCm { get; set; } = 170;
        public double WeightKg { get; set; } = 60;
        public string Alignment { get; set; } = "Истинный нейтрал";

        public string RankStrength { get; set; } = "E";
        public string RankEndurance { get; set; } = "E";
        public string RankAgility { get; set; } = "E";
        public string RankMagicEnergy { get; set; } = "E";
        public string RankLuck { get; set; } = "E";
        public string RankNoblePhantasm { get; set; } = "E";

        // Текущие показатели (сохраняются между боями)
        public int CurrentHP { get; set; }
        public int CurrentMana { get; set; }

        // Время последней синхронизации регенерации
        public DateTime LastSync { get; set; } = DateTime.Now;

        public List<Skill> Skills { get; set; } = new();
        public List<Skill> NoblePhantasms { get; set; } = new();

        public int MaxHP => RankTables.GetHp(RankEndurance);
        public int MaxMana => RankTables.GetMana(RankMagicEnergy);
        public int AttackDamage => RankTables.GetAttack(RankStrength);

        public bool IsMaster => ClassType == "Мастер";

        /// <summary>
        /// Восполняет HP и ману в соответствии с прошедшим реальным временем
        /// с момента LastSync. Сейбер/Лансер/.../Экстра: 5HP и 3 Маны/мин.
        /// Мастер: 2HP и 2 Маны/мин.
        /// </summary>
        public void SyncRegeneration()
        {
            var now = DateTime.Now;
            var minutes = (now - LastSync).TotalMinutes;
            if (minutes <= 0) { LastSync = now; return; }

            double hpRate = IsMaster ? 2 : 5;
            double manaRate = IsMaster ? 2 : 3;

            CurrentHP = Math.Min(MaxHP, CurrentHP + (int)Math.Floor(minutes * hpRate));
            CurrentMana = Math.Min(MaxMana, CurrentMana + (int)Math.Floor(minutes * manaRate));
            LastSync = now;
        }

        public void ResetToFull()
        {
            CurrentHP = MaxHP;
            CurrentMana = MaxMana;
            LastSync = DateTime.Now;
        }
    }
}
