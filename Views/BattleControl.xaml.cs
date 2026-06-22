using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FateCalculator.Data;
using FateCalculator.Models;

namespace FateCalculator.Views
{
    public partial class BattleControl : UserControl
    {
        private readonly BattleEncounter _encounter;
        private readonly List<Character> _allCharacters;

        public BattleControl(BattleEncounter encounter, List<Character> allCharacters)
        {
            InitializeComponent();
            _encounter = encounter;
            _allCharacters = allCharacters;
            CharacterPicker.ItemsSource = _allCharacters;
            Refresh();
        }

        private void AddParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterPicker.SelectedItem is Character c && !_encounter.Participants.Contains(c))
            {
                c.SyncRegeneration();
                _encounter.AddParticipant(c);
                Refresh();
            }
        }

        private void RemoveParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (ParticipantsListBox.SelectedItem is Character c)
            {
                _encounter.RemoveParticipant(c);
                Refresh();
            }
        }

        private void NextTurn_Click(object sender, RoutedEventArgs e)
        {
            _encounter.NextTurn();
            Refresh();
        }

        private void EndBattle_Click(object sender, RoutedEventArgs e)
        {
            _encounter.EndBattle();
            foreach (var c in _encounter.Participants)
                Database.Save(c);
            MessageBox.Show("Бой завершён. Текущие HP/Мана сохранены, регенерация начнёт отсчёт реального времени.",
                "Бой окончен", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Refresh()
        {
            ParticipantsListBox.ItemsSource = null;
            ParticipantsListBox.ItemsSource = _encounter.Participants;

            RoundLabel.Text = $"Раунд: {_encounter.Round}";
            var actor = _encounter.CurrentActor;
            CurrentTurnLabel.Text = actor == null ? "Сейчас ходит: —" : $"Сейчас ходит: {actor.ClassType} — {actor.Name}";
            TurnOrderLabel.Text = _encounter.Participants.Count == 0 ? "Очередь: —" : $"Очередь ходов: {_encounter.TurnOrderDisplay}";

            ParticipantCardsList.Items.Clear();
            foreach (var c in _encounter.Participants)
                ParticipantCardsList.Items.Add(BuildParticipantCard(c, actor == c));
        }

        private UIElement BuildParticipantCard(Character c, bool isActive)
        {
            var border = new Border
            {
                BorderBrush = isActive ? (Brush)FindResource("GoldAccent") : (Brush)FindResource("GoldSoft"),
                BorderThickness = new Thickness(isActive ? 2 : 1),
                Background = (Brush)FindResource("BgPanel"),
                Margin = new Thickness(0, 0, 0, 8),
                Padding = new Thickness(10)
            };

            var stack = new StackPanel();
            var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
            titleRow.Children.Add(new TextBlock
            {
                Text = $"{c.Name}  [{c.ClassType}]  (Ловкость: {c.RankAgility})",
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("GoldAccent"),
                FontSize = 15
            });
            if (isActive)
                titleRow.Children.Add(new TextBlock { Text = "  ◀ ХОДИТ", Foreground = Brushes.OrangeRed, FontWeight = FontWeights.Bold });
            stack.Children.Add(titleRow);

            stack.Children.Add(MakeBar("HP", c.CurrentHP, c.MaxHP, Colors.Crimson));
            stack.Children.Add(MakeBar("Мана", c.CurrentMana, c.MaxMana, Colors.RoyalBlue));

            var dmgRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
            dmgRow.Children.Add(new TextBlock { Text = $"Атака: {c.AttackDamage}   ", VerticalAlignment = VerticalAlignment.Center });

            var attackBtn = new Button { Content = "Применить обычную атаку (к себе как тест/лог нет цели)", Margin = new Thickness(0, 0, 6, 0) };
            // Простая атака не наносит урон самому себе — кнопка лишь фиксирует использование (трекинг урона ведётся вручную ведущим/ГМ)
            dmgRow.Children.Add(attackBtn);
            stack.Children.Add(dmgRow);

            if (c.Skills.Count > 0 || c.NoblePhantasms.Count > 0)
            {
                stack.Children.Add(new TextBlock { Text = "Навыки и Небесный Фантазм:", Margin = new Thickness(0, 8, 0, 4), Style = (Style)FindResource("SubText") });
                foreach (var sk in c.Skills.Concat(c.NoblePhantasms))
                {
                    var skRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
                    skRow.Children.Add(new TextBlock { Text = $"{sk.Name} ", VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.SemiBold });
                    skRow.Children.Add(new TextBlock { Text = sk.ManaCost > 0 ? $"({sk.ManaCost} маны)  " : "(бесплатно)  ", Style = (Style)FindResource("SubText"), VerticalAlignment = VerticalAlignment.Center });
                    var useBtn = new Button { Content = "Использовать" };
                    useBtn.Click += (s, e) =>
                    {
                        if (sk.ManaCost > 0)
                        {
                            if (c.CurrentMana < sk.ManaCost)
                            {
                                MessageBox.Show($"У {c.Name} недостаточно маны для «{sk.Name}» ({c.CurrentMana}/{sk.ManaCost}).");
                                return;
                            }
                            c.CurrentMana -= sk.ManaCost;
                        }
                        Database.Save(c);
                        Refresh();
                    };
                    skRow.Children.Add(useBtn);
                    stack.Children.Add(skRow);
                }
            }

            border.Child = stack;
            return border;
        }

        private UIElement MakeBar(string label, int current, int max, Color color)
        {
            var stack = new StackPanel { Margin = new Thickness(0, 2, 0, 2) };
            stack.Children.Add(new TextBlock { Text = $"{label}: {current} / {max}", FontSize = 12 });
            var grid = new Grid { Width = 260, Height = 12 };
            var outer = new Border { Background = (Brush)FindResource("BgPanelLight"), CornerRadius = new CornerRadius(3) };
            double pct = max <= 0 ? 0 : System.Math.Max(0, System.Math.Min(1.0, (double)current / max));
            var inner = new Border { Background = new SolidColorBrush(color), HorizontalAlignment = HorizontalAlignment.Left, Width = 260 * pct, CornerRadius = new CornerRadius(3) };
            grid.Children.Add(outer);
            grid.Children.Add(inner);
            stack.Children.Add(grid);
            return stack;
        }
    }
}
