using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FateCalculator.Data;
using FateCalculator.Models;
using FateCalculator.Views;

namespace FateCalculator
{
    public partial class MainWindow : Window
    {
        private List<Character> _characters = new();
        private int _battleCounter = 0;

        public MainWindow()
        {
            InitializeComponent();
            ReloadCharacters();
        }

        private void ReloadCharacters()
        {
            _characters = Database.LoadAll();
            CharacterListBox.ItemsSource = null;
            CharacterListBox.ItemsSource = _characters;
        }

        private void NewCharacter_Click(object sender, RoutedEventArgs e)
        {
            var editor = new CharacterEditorWindow(null);
            if (editor.ShowDialog() == true)
            {
                ReloadCharacters();
            }
        }

        private void CharacterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CharacterListBox.SelectedItem is Character c)
            {
                c.SyncRegeneration();
                Database.Save(c);
                RenderCharacterDetail(c);
            }
        }

        private void RenderCharacterDetail(Character c)
        {
            var root = new StackPanel { Margin = new Thickness(0) };

            // --- Заголовок: картинка + основные данные ---
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var imageBorder = new Border
            {
                Width = 200, Height = 260,
                BorderBrush = (Brush)FindResource("GoldSoft"),
                BorderThickness = new Thickness(2),
                Background = (Brush)FindResource("BgPanel")
            };
            if (!string.IsNullOrWhiteSpace(c.ImagePath) && System.IO.File.Exists(c.ImagePath))
            {
                try
                {
                    var img = new Image
                    {
                        Source = new BitmapImage(new Uri(c.ImagePath)),
                        Stretch = Stretch.UniformToFill
                    };
                    imageBorder.Child = img;
                }
                catch { }
            }
            else
            {
                imageBorder.Child = new TextBlock
                {
                    Text = "Нет изображения",
                    Style = (Style)FindResource("SubText"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
            Grid.SetColumn(imageBorder, 0);
            headerGrid.Children.Add(imageBorder);

            var infoStack = new StackPanel { Margin = new Thickness(15, 0, 0, 0) };
            infoStack.Children.Add(new TextBlock { Text = c.Name, Style = (Style)FindResource("HeaderText"), FontSize = 26 });
            infoStack.Children.Add(MakeInfoLine("Класс", c.ClassType));
            infoStack.Children.Add(MakeInfoLine("Рост / Вес", $"{c.HeightCm} см / {c.WeightKg} кг"));
            infoStack.Children.Add(MakeInfoLine("Мировоззрение", c.Alignment));
            Grid.SetColumn(infoStack, 1);
            headerGrid.Children.Add(infoStack);

            root.Children.Add(headerGrid);
            root.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

            // --- Параметры (ранги) ---
            root.Children.Add(new TextBlock { Text = "Параметры", Style = (Style)FindResource("HeaderText") });
            var paramsGrid = new Grid();
            for (int i = 0; i < 2; i++) paramsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            string[] paramNames = { "Сила", "Выносливость", "Ловкость", "Магическая Энергия", "Удача", "Небесный Фантазм" };
            string[] paramVals = { c.RankStrength, c.RankEndurance, c.RankAgility, c.RankMagicEnergy, c.RankLuck, c.RankNoblePhantasm };
            for (int i = 0; i < paramNames.Length; i++)
            {
                paramsGrid.RowDefinitions.Add(new RowDefinition());
                var line = MakeInfoLine(paramNames[i], paramVals[i]);
                Grid.SetRow(line, i);
                paramsGrid.Children.Add(line);
            }
            root.Children.Add(paramsGrid);

            root.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

            // --- Боевые показатели ---
            root.Children.Add(new TextBlock { Text = "Боевые показатели", Style = (Style)FindResource("HeaderText") });
            root.Children.Add(MakeStatBar("HP", c.CurrentHP, c.MaxHP, Colors.Crimson));
            root.Children.Add(MakeStatBar("Мана", c.CurrentMana, c.MaxMana, Colors.RoyalBlue));
            root.Children.Add(MakeInfoLine("Обычная атака (урон)", c.AttackDamage.ToString()));
            root.Children.Add(MakeInfoLine("Последняя синхронизация", c.LastSync.ToString("g")));

            var syncBtn = new Button { Content = "↻ Синхронизировать регенерацию сейчас", Margin = new Thickness(0, 8, 0, 0), HorizontalAlignment = HorizontalAlignment.Left };
            syncBtn.Click += (s, e) => { c.SyncRegeneration(); Database.Save(c); RenderCharacterDetail(c); };
            root.Children.Add(syncBtn);

            var resetBtn = new Button { Content = "Восстановить HP/Ману полностью", Margin = new Thickness(0, 6, 0, 0), HorizontalAlignment = HorizontalAlignment.Left };
            resetBtn.Click += (s, e) => { c.ResetToFull(); Database.Save(c); RenderCharacterDetail(c); };
            root.Children.Add(resetBtn);

            root.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

            // --- Навыки и Небесные Фантазмы ---
            root.Children.Add(new TextBlock { Text = "Навыки", Style = (Style)FindResource("HeaderText") });
            foreach (var sk in c.Skills) root.Children.Add(MakeSkillCard(sk));
            if (c.Skills.Count == 0) root.Children.Add(new TextBlock { Text = "Навыков не добавлено", Style = (Style)FindResource("SubText") });

            root.Children.Add(new TextBlock { Text = "Небесный Фантазм", Style = (Style)FindResource("HeaderText"), Margin = new Thickness(0, 15, 0, 8) });
            foreach (var np in c.NoblePhantasms) root.Children.Add(MakeSkillCard(np));
            if (c.NoblePhantasms.Count == 0) root.Children.Add(new TextBlock { Text = "Небесные Фантазмы не добавлены", Style = (Style)FindResource("SubText") });

            root.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

            var editBtn = new Button { Content = "✎ Редактировать персонажа", HorizontalAlignment = HorizontalAlignment.Left };
            editBtn.Click += (s, e) =>
            {
                var editor = new CharacterEditorWindow(c);
                if (editor.ShowDialog() == true)
                {
                    ReloadCharacters();
                }
            };
            var deleteBtn = new Button { Content = "✕ Удалить персонажа", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 8, 0, 0) };
            deleteBtn.Click += (s, e) =>
            {
                if (MessageBox.Show($"Удалить персонажа «{c.Name}»?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Database.Delete(c.Id);
                    ReloadCharacters();
                    CharacterDetailPanel.Children.Clear();
                }
            };
            root.Children.Add(editBtn);
            root.Children.Add(deleteBtn);

            CharacterDetailPanel.Children.Clear();
            CharacterDetailPanel.Children.Add(root);
        }

        private UIElement MakeInfoLine(string label, string value)
        {
            var p = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            p.Children.Add(new TextBlock { Text = label + ": ", Style = (Style)FindResource("SubText"), Width = 170 });
            p.Children.Add(new TextBlock { Text = value, FontWeight = FontWeights.Bold });
            return p;
        }

        private UIElement MakeStatBar(string label, int current, int max, Color color)
        {
            var stack = new StackPanel { Margin = new Thickness(0, 4, 0, 4) };
            stack.Children.Add(new TextBlock { Text = $"{label}: {current} / {max}" });
            var outer = new Border { Background = (Brush)FindResource("BgPanelLight"), Height = 14, CornerRadius = new CornerRadius(3), Margin = new Thickness(0, 2, 0, 0) };
            double pct = max <= 0 ? 0 : Math.Max(0, Math.Min(1.0, (double)current / max));
            var inner = new Border
            {
                Background = new SolidColorBrush(color),
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 300 * pct,
                CornerRadius = new CornerRadius(3)
            };
            var grid = new Grid { Width = 300 };
            grid.Children.Add(outer);
            grid.Children.Add(inner);
            stack.Children.Add(grid);
            return stack;
        }

        private UIElement MakeSkillCard(Skill sk)
        {
            var border = new Border
            {
                BorderBrush = (Brush)FindResource("GoldSoft"),
                BorderThickness = new Thickness(1),
                Background = (Brush)FindResource("BgPanel"),
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(8)
            };
            var stack = new StackPanel();
            var titleLine = new StackPanel { Orientation = Orientation.Horizontal };
            titleLine.Children.Add(new TextBlock { Text = sk.Name, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("GoldAccent") });
            titleLine.Children.Add(new TextBlock
            {
                Text = sk.ManaCost > 0 ? $"   (стоимость: {sk.ManaCost} маны)" : "   (бесплатно)",
                Style = (Style)FindResource("SubText")
            });
            stack.Children.Add(titleLine);
            stack.Children.Add(new TextBlock { Text = sk.Description, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0) });
            border.Child = stack;
            return border;
        }

        // ===================== БОИ =====================

        private void NewBattle_Click(object sender, RoutedEventArgs e)
        {
            _battleCounter++;
            var encounter = new BattleEncounter { Title = $"Бой #{_battleCounter}" };
            var control = new BattleControl(encounter, _characters);

            var tab = new TabItem();
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerPanel.Children.Add(new TextBlock { Text = encounter.Title, VerticalAlignment = VerticalAlignment.Center });
            var closeBtn = new Button { Content = "✕", Margin = new Thickness(8, 0, 0, 0), Padding = new Thickness(4, 0, 4, 0) };
            closeBtn.Click += (s, ev) => BattleTabs.Items.Remove(tab);
            headerPanel.Children.Add(closeBtn);
            tab.Header = headerPanel;
            tab.Content = control;

            BattleTabs.Items.Add(tab);
            BattleTabs.SelectedItem = tab;
        }
    }
}
