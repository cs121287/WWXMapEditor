using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WwXMapEditor.Models;

namespace WwXMapEditor
{
    public partial class PlayerManagementWindow : Window
    {
        public ObservableCollection<Player> Players { get; }
        public ObservableCollection<string> Countries { get; }
        public ObservableCollection<string> Colors { get; }

        private ObservableCollection<string> _usedColors;

        public PlayerManagementWindow(ObservableCollection<Player> existingPlayers)
        {
            InitializeComponent();
            DataContext = this;

            Players = new ObservableCollection<Player>();
            _usedColors = new ObservableCollection<string>();

            foreach (var player in existingPlayers)
            {
                var newPlayer = new Player
                {
                    Name = player.Name,
                    Country = player.Country,
                    IsAI = player.IsAI,
                    Color = player.Color
                };
                Players.Add(newPlayer);

                // Track used colors
                if (!_usedColors.Contains(player.Color))
                {
                    _usedColors.Add(player.Color);
                }
            }

            Countries = new ObservableCollection<string>
            {
                "Redonia",     // Blitzkrieg - +20% attack for all ground units; +2 movement for tanks
                "Sevaria",     // Industrial Surge - +50% property income; units cost 20% less to build
                "Auria",       // Air Supremacy - All air units gain +30% attack; vision increased map-wide
                "Maritain",    // Naval Dominion - All ships gain +40% attack, +2 movement; reveal all sea tiles
                "Norvos",      // Defensive Wall - All units gain +30% defense; properties cannot be captured
                "Valtros",     // Recon Sweep - Reveal all enemy units on map for all players
                "Unspecified"  // For neutral or custom scenarios
            };

            Colors = new ObservableCollection<string>
            {
                "Blue", "Red", "Green", "Yellow", "Orange", "Purple", "Cyan", "Pink"
            };

            PlayersGrid.ItemsSource = Players;

            // Subscribe to collection changed events
            Players.CollectionChanged += Players_CollectionChanged;

            // Subscribe to property changed events for existing players
            foreach (var player in Players)
            {
                player.PropertyChanged += Player_PropertyChanged;
            }
        }

        private void Players_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Player player in e.NewItems)
                {
                    player.PropertyChanged += Player_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Player player in e.OldItems)
                {
                    player.PropertyChanged -= Player_PropertyChanged;
                    // Remove color from used colors if no other player is using it
                    if (!Players.Any(p => p != player && p.Color == player.Color))
                    {
                        _usedColors.Remove(player.Color);
                    }
                }
            }
        }

        private void Player_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is Player player && e.PropertyName == nameof(Player.Color))
            {
                // Update used colors tracking
                UpdateUsedColors();
            }
        }

        private void UpdateUsedColors()
        {
            _usedColors.Clear();
            foreach (var player in Players)
            {
                if (!string.IsNullOrEmpty(player.Color) && !_usedColors.Contains(player.Color))
                {
                    _usedColors.Add(player.Color);
                }
            }
        }

        private void AddPlayer_Click(object sender, RoutedEventArgs e)
        {
            // Find the first available color
            var availableColor = Colors.FirstOrDefault(c => !_usedColors.Contains(c));
            if (availableColor == null)
            {
                MessageBox.Show("All colors are already in use. Please remove a player or change an existing player's color first.",
                    "No Available Colors", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newPlayer = new Player
            {
                Name = $"Player {Players.Count + 1}",
                Country = "Unspecified",
                IsAI = false,
                Color = availableColor
            };

            Players.Add(newPlayer);
            _usedColors.Add(availableColor);
        }

        private void RemovePlayer_Click(object sender, RoutedEventArgs e)
        {
            if (PlayersGrid.SelectedItem is Player player)
            {
                if (Players.Count == 1)
                {
                    MessageBox.Show("You must have at least one player.", "Cannot Remove Player",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Players.Remove(player);

                // Update used colors
                if (!Players.Any(p => p.Color == player.Color))
                {
                    _usedColors.Remove(player.Color);
                }
            }
            else
            {
                MessageBox.Show("Please select a player to remove.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool ValidatePlayers()
        {
            // Check for empty player names
            for (int i = 0; i < Players.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(Players[i].Name))
                {
                    MessageBox.Show($"Player {i + 1} has an empty name. Please enter a name for all players.",
                        "Invalid Player Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            // Check for duplicate player names
            var duplicateNames = Players.GroupBy(p => p.Name.Trim())
                                       .Where(g => g.Count() > 1)
                                       .Select(g => g.Key)
                                       .ToList();

            if (duplicateNames.Any())
            {
                MessageBox.Show($"Duplicate player names found: {string.Join(", ", duplicateNames)}. " +
                    "Each player must have a unique name.",
                    "Duplicate Names", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check for duplicate colors
            var duplicateColors = Players.GroupBy(p => p.Color)
                                        .Where(g => g.Count() > 1)
                                        .ToList();

            if (duplicateColors.Any())
            {
                var message = "Multiple players have the same color:\n";
                foreach (var group in duplicateColors)
                {
                    var playerNames = string.Join(", ", group.Select(p => p.Name));
                    message += $"\n{group.Key}: {playerNames}";
                }
                message += "\n\nEach player must have a unique color.";

                MessageBox.Show(message, "Duplicate Colors", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check that at least one player exists
            if (Players.Count == 0)
            {
                MessageBox.Show("You must have at least one player.", "No Players",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate countries
            foreach (var player in Players)
            {
                if (string.IsNullOrWhiteSpace(player.Country))
                {
                    player.Country = "Unspecified";
                }
            }

            return true;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (ValidatePlayers())
            {
                // Trim player names before saving
                foreach (var player in Players)
                {
                    player.Name = player.Name.Trim();
                }

                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from events to prevent memory leaks
            Players.CollectionChanged -= Players_CollectionChanged;
            foreach (var player in Players)
            {
                player.PropertyChanged -= Player_PropertyChanged;
            }

            base.OnClosed(e);
        }
    }
}