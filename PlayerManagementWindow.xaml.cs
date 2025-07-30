using System.Collections.ObjectModel;
using System.Windows;
using WwXMapEditor.Models;

namespace WwXMapEditor
{
    public partial class PlayerManagementWindow : Window
    {
        public ObservableCollection<Player> Players { get; }
        public ObservableCollection<string> Countries { get; }
        public ObservableCollection<string> Colors { get; }

        public PlayerManagementWindow(ObservableCollection<Player> existingPlayers)
        {
            InitializeComponent();
            DataContext = this;

            Players = new ObservableCollection<Player>();
            foreach (var player in existingPlayers)
            {
                Players.Add(new Player
                {
                    Name = player.Name,
                    Country = player.Country,
                    IsAI = player.IsAI,
                    Color = player.Color
                });
            }

            Countries = new ObservableCollection<string>
            {
                "USA", "Russia", "Germany", "Britain", "France", "Japan", "China", "Unspecified"
            };

            Colors = new ObservableCollection<string>
            {
                "Blue", "Red", "Green", "Yellow", "Orange", "Purple", "Cyan", "Pink"
            };

            PlayersGrid.ItemsSource = Players;
        }

        private void AddPlayer_Click(object sender, RoutedEventArgs e)
        {
            Players.Add(new Player
            {
                Name = $"Player {Players.Count + 1}",
                Country = "Unspecified",
                IsAI = false,
                Color = "Blue"
            });
        }

        private void RemovePlayer_Click(object sender, RoutedEventArgs e)
        {
            if (PlayersGrid.SelectedItem is Player player)
            {
                Players.Remove(player);
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
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