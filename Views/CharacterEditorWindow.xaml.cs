using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using FateCalculator.Data;
using FateCalculator.Models;

namespace FateCalculator.Views
{
    public partial class CharacterEditorWindow : Window
    {
        private readonly Character _character;
        private readonly bool _isNew;

        public CharacterEditorWindow(Character existing)
        {
            InitializeComponent();

            _isNew = existing == null;
            _character = existing ?? new Character();

            ClassBox.ItemsSource = Lists.Classes;
            AlignmentBox.ItemsSource = Lists.Alignments;
            StrengthBox.ItemsSource = Lists.RankOrder;
            EnduranceBox.ItemsSource = Lists.RankOrder;
            AgilityBox.ItemsSource = Lists.RankOrder;
            MagicBox.ItemsSource = Lists.RankOrder;
            LuckBox.ItemsSource = Lists.RankOrder;
            NpRankBox.ItemsSource = Lists.RankOrder;

            LoadFromCharacter();
        }

        private void LoadFromCharacter()
        {
            NameBox.Text = _character.Name;
            ClassBox.SelectedItem = _character.ClassType;
            HeightBox.Text = _character.HeightCm.ToString(CultureInfo.InvariantCulture);
            WeightBox.Text = _character.WeightKg.ToString(CultureInfo.InvariantCulture);
            AlignmentBox.SelectedItem = _character.Alignment;

            StrengthBox.SelectedItem = _character.RankStrength;
            EnduranceBox.SelectedItem = _character.RankEndurance;
            AgilityBox.SelectedItem = _character.RankAgility;
            MagicBox.SelectedItem = _character.RankMagicEnergy;
            LuckBox.SelectedItem = _character.RankLuck;
            NpRankBox.SelectedItem = _character.RankNoblePhantasm;

            CurrentHpBox.Text = (_isNew ? _character.MaxHP : _character.CurrentHP).ToString();
            CurrentManaBox.Text = (_isNew ? _character.MaxMana : _character.CurrentMana).ToString();

            if (!string.IsNullOrWhiteSpace(_character.ImagePath) && System.IO.File.Exists(_character.ImagePath))
            {
                try { PreviewImage.Source = new BitmapImage(new Uri(_character.ImagePath)); } catch { }
            }

            RefreshSkillsUI();
        }

        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.webp" };
            if (dlg.ShowDialog() == true)
            {
                _character.ImagePath = dlg.FileName;
                PreviewImage.Source = new BitmapImage(new Uri(dlg.FileName));
            }
        }

        private void RefreshSkillsUI()
        {
            SkillsList.Items.Clear();
            foreach (var sk in _character.Skills)
                SkillsList.Items.Add(BuildSkillEditorRow(sk, _character.Skills));

            NpList.Items.Clear();
            foreach (var np in _character.NoblePhantasms)
                NpList.Items.Add(BuildSkillEditorRow(np, _character.NoblePhantasms));
        }

        private UIElement BuildSkillEditorRow(Skill sk, System.Collections.Generic.List<Skill> owningList)
        {
            var border = new Border
            {
                BorderBrush = (Brush)FindResource("GoldSoft"),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(8),
                Background = (Brush)FindResource("BgPanel")
            };
            var stack = new StackPanel();

            var nameRow = new Grid();
            nameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            nameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            nameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

            var nameBox = new TextBox { Text = sk.Name };
            nameBox.TextChanged += (s, e) => sk.Name = nameBox.Text;
            Grid.SetColumn(nameBox, 0);

            var costBox = new TextBox { Text = sk.ManaCost.ToString(), Margin = new Thickness(6, 0, 6, 0) };
            costBox.TextChanged += (s, e) => int.TryParse(costBox.Text,out sk.ManaCost);
            Grid.SetColumn(costBox, 1);

            var removeBtn = new Button { Content = "✕" };
            removeBtn.Click += (s, e) => { owningList.Remove(sk); RefreshSkillsUI(); };
            Grid.SetColumn(removeBtn, 2);

            nameRow.Children.Add(nameBox);
            nameRow.Children.Add(costBox);
            nameRow.Children.Add(removeBtn);

            var descBox = new TextBox { Text = sk.Description, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0), MinLines = 2 };
            descBox.TextChanged += (s, e) => sk.Description = descBox.Text;

            stack.Children.Add(new TextBlock { Text = "Название / Стоимость маны (0 = бесплатно):", Style = (Style)FindResource("SubText") });
            stack.Children.Add(nameRow);
            stack.Children.Add(new TextBlock { Text = "Описание:", Style = (Style)FindResource("SubText"), Margin = new Thickness(0, 4, 0, 0) });
            stack.Children.Add(descBox);

            border.Child = stack;
            return border;
        }

        private void AddSkill_Click(object sender, RoutedEventArgs e)
        {
            _character.Skills.Add(new Skill { Name = "Новый навык" });
            RefreshSkillsUI();
        }

        private void AddNp_Click(object sender, RoutedEventArgs e)
        {
            _character.NoblePhantasms.Add(new Skill { Name = "Новый Небесный Фантазм", IsNoblePhantasm = true });
            RefreshSkillsUI();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Укажите имя персонажа.");
                return;
            }

            _character.Name = NameBox.Text.Trim();
            _character.ClassType = ClassBox.SelectedItem as string ?? "Сейбер";
            _character.Alignment = AlignmentBox.SelectedItem as string ?? "Истинный нейтрал";

            double.TryParse(HeightBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var h);
            double.TryParse(WeightBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var w);
            _character.HeightCm = h;
            _character.WeightKg = w;

            _character.RankStrength = StrengthBox.SelectedItem as string ?? "E";
            _character.RankEndurance = EnduranceBox.SelectedItem as string ?? "E";
            _character.RankAgility = AgilityBox.SelectedItem as string ?? "E";
            _character.RankMagicEnergy = MagicBox.SelectedItem as string ?? "E";
            _character.RankLuck = LuckBox.SelectedItem as string ?? "E";
            _character.RankNoblePhantasm = NpRankBox.SelectedItem as string ?? "E";

            int.TryParse(CurrentHpBox.Text, out var hp);
            int.TryParse(CurrentManaBox.Text, out var mana);
            _character.CurrentHP = Math.Min(hp, _character.MaxHP);
            _character.CurrentMana = Math.Min(mana, _character.MaxMana);
            _character.LastSync = DateTime.Now;

            Database.Save(_character);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
