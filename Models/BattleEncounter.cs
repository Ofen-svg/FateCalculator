using System;
using System.Collections.Generic;
using System.Linq;

namespace FateCalculator.Models
{
    public class BattleEncounter
    {
        public string Title { get; set; } = "Новый бой";
        public List<Character> Participants { get; } = new();
        public int Round { get; private set; } = 1;
        public int CurrentTurnIndex { get; private set; } = 0;

        /// <summary>
        /// Порядок ходов: по убыванию ранга Ловкости. При равенстве рангов
        /// сохраняется порядок добавления в бой.
        /// </summary>
        public List<Character> TurnOrder =>
            Participants
                .Select((c, idx) => (c, idx))
                .OrderByDescending(t => Lists.RankIndex(t.c.RankAgility))
                .ThenBy(t => t.idx)
                .Select(t => t.c)
                .ToList();

        public Character CurrentActor
        {
            get
            {
                var order = TurnOrder;
                if (order.Count == 0) return null;
                if (CurrentTurnIndex >= order.Count) CurrentTurnIndex = 0;
                return order[CurrentTurnIndex];
            }
        }

        public string TurnOrderDisplay =>
            string.Join("  >  ", TurnOrder.Select(c => $"{c.ClassType} ({c.Name})"));

        public void AddParticipant(Character c) => Participants.Add(c);

        public void RemoveParticipant(Character c)
        {
            Participants.Remove(c);
            if (CurrentTurnIndex >= Participants.Count) CurrentTurnIndex = 0;
        }

        /// <summary>Переход к следующему ходу. Если круг закончился — новый раунд.</summary>
        public void NextTurn()
        {
            if (Participants.Count == 0) return;
            CurrentTurnIndex++;
            if (CurrentTurnIndex >= TurnOrder.Count)
            {
                CurrentTurnIndex = 0;
                Round++;
            }
        }

        /// <summary>Завершение боя: запускаем счётчик реального времени для регенерации.</summary>
        public void EndBattle()
        {
            foreach (var c in Participants)
                c.LastSync = DateTime.Now;
        }
    }
}
