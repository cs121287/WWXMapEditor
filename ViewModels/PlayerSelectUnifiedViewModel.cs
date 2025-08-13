using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace WWXMapEditor.ViewModels
{
    public class PlayerSelectUnifiedViewModel : INotifyPropertyChanged
    {
        // Countries (include "Random" first)
        private static readonly string[] AllCountries =
        {
            "Random", "Redonia", "Sevaria", "Auria", "Maritain", "Norvos", "Valtro"
        };

        public ObservableCollection<MatchupOption> MatchupOptions { get; } = new ObservableCollection<MatchupOption>
        {
            new MatchupOption("1 vs 1", new[] {1, 2}),
            new MatchupOption("1 vs 2", new[] {1, 2, 2}),
            new MatchupOption("2 vs 2", new[] {1, 1, 2, 2}),
            new MatchupOption("2 vs 1", new[] {1, 1, 2}),
            new MatchupOption("3 vs 1", new[] {1, 1, 1, 2}),
            new MatchupOption("1 vs 3", new[] {1, 2, 2, 2}),
        };

        private MatchupOption? _selectedMatchup;
        public MatchupOption? SelectedMatchup
        {
            get => _selectedMatchup;
            set
            {
                if (_selectedMatchup != value)
                {
                    _selectedMatchup = value;
                    OnPropertyChanged(nameof(SelectedMatchup));
                    ApplyMatchup(_selectedMatchup ?? MatchupOptions[0]);
                }
            }
        }

        public ObservableCollection<PlayerSlotViewModel> Players { get; } = new ObservableCollection<PlayerSlotViewModel>();

        private string _validationMessage = "";
        public string ValidationMessage
        {
            get => _validationMessage;
            set { if (_validationMessage != value) { _validationMessage = value; OnPropertyChanged(nameof(ValidationMessage)); } }
        }

        public string SummaryText => BuildSummary();

        // Compatibility properties (if the outer wizard relies on them)
        private int _numberOfPlayers = 2;
        public int NumberOfPlayers
        {
            get => _numberOfPlayers;
            private set { if (_numberOfPlayers != value) { _numberOfPlayers = value; OnPropertyChanged(nameof(NumberOfPlayers)); } }
        }

        public string Player1Country
        {
            get => GetPlayerCountry(1);
            set => SetPlayerCountry(1, value);
        }
        public string Player2Country
        {
            get => GetPlayerCountry(2);
            set => SetPlayerCountry(2, value);
        }
        public string Player3Country
        {
            get => GetPlayerCountry(3);
            set => SetPlayerCountry(3, value);
        }
        public string Player4Country
        {
            get => GetPlayerCountry(4);
            set => SetPlayerCountry(4, value);
        }

        public bool Player2IsAlly
        {
            get => GetIsAlly(2);
            private set => SetIsAlly(2, value);
        }
        public bool Player3IsAlly
        {
            get => GetIsAlly(3);
            private set => SetIsAlly(3, value);
        }
        public bool Player4IsAlly
        {
            get => GetIsAlly(4);
            private set => SetIsAlly(4, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PlayerSelectUnifiedViewModel()
        {
            // Initialize with default 1 vs 1
            SelectedMatchup = MatchupOptions[0];
        }

        private void ApplyMatchup(MatchupOption matchup)
        {
            // Adjust player slots to match the matchup length
            var required = matchup.TeamAssignments.Length;

            // Grow
            while (Players.Count < required)
            {
                var index = Players.Count + 1;
                var player = new PlayerSlotViewModel(index);
                player.PropertyChanged += PlayerOnPropertyChanged;
                // Default selection is "Random"
                player.SelectedCountry = "Random";
                Players.Add(player);
            }
            // Shrink
            while (Players.Count > required)
            {
                var idx = Players.Count - 1;
                var player = Players[idx];
                player.PropertyChanged -= PlayerOnPropertyChanged;
                Players.RemoveAt(idx);
            }

            // Apply teams
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].TeamNumber = matchup.TeamAssignments[i];
            }

            // Update compatibility properties
            NumberOfPlayers = Players.Count;
            Player2IsAlly = Players.Count >= 2 && (Players[1].TeamNumber == Players[0].TeamNumber);
            Player3IsAlly = Players.Count >= 3 && (Players[2].TeamNumber == Players[0].TeamNumber);
            Player4IsAlly = Players.Count >= 4 && (Players[3].TeamNumber == Players[0].TeamNumber);

            UpdateAvailability();
            Validate();
            OnPropertyChanged(nameof(SummaryText));
        }

        private void PlayerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayerSlotViewModel.SelectedCountry))
            {
                // Enforce uniqueness: if two players now share the same non-Random selection,
                // reset the older occurrence(s) to Random keeping the latest change.
                var latest = sender as PlayerSlotViewModel;
                if (latest is not null)
                {
                    string choice = latest.SelectedCountry;
                    if (!string.Equals(choice, "Random", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var p in Players)
                        {
                            if (!ReferenceEquals(p, latest) &&
                                string.Equals(p.SelectedCountry, choice, StringComparison.OrdinalIgnoreCase))
                            {
                                p.SelectedCountry = "Random";
                            }
                        }
                    }
                }

                // Keep compatibility properties in sync
                OnPropertyChanged(nameof(Player1Country));
                OnPropertyChanged(nameof(Player2Country));
                OnPropertyChanged(nameof(Player3Country));
                OnPropertyChanged(nameof(Player4Country));

                UpdateAvailability();
                Validate();
            }
        }

        private void UpdateAvailability()
        {
            // Build set of selected (non-Random)
            var selected = new HashSet<string>(
                Players.Select(p => p.SelectedCountry)
                       .Where(c => !string.IsNullOrWhiteSpace(c) && !c.Equals("Random", StringComparison.OrdinalIgnoreCase)),
                StringComparer.OrdinalIgnoreCase);

            foreach (var p in Players)
            {
                var keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Random" };

                foreach (var c in AllCountries.Skip(1)) // skip "Random" as it's already added
                {
                    // If another player has selected it, hide from this player.
                    bool takenByOther = selected.Contains(c) && !string.Equals(p.SelectedCountry, c, StringComparison.OrdinalIgnoreCase);
                    if (!takenByOther)
                        keep.Add(c);
                }

                // Ensure currently selected remains present (if non-random and temporarily conflicting, it will be reset above)
                if (!string.IsNullOrWhiteSpace(p.SelectedCountry))
                    keep.Add(p.SelectedCountry);

                // Apply to the observable list
                p.ReplaceAvailableWith(keep.OrderBy(s => s.Equals("Random", StringComparison.OrdinalIgnoreCase) ? "" : s).ToList());
            }
        }

        private void Validate()
        {
            // No duplicate non-random countries and every player's selection must be non-empty (Random allowed)
            var dup = Players.Where(p => !string.Equals(p.SelectedCountry, "Random", StringComparison.OrdinalIgnoreCase))
                             .GroupBy(p => p.SelectedCountry, StringComparer.OrdinalIgnoreCase)
                             .FirstOrDefault(g => g.Count() > 1);
            if (dup != null)
            {
                ValidationMessage = $"Country '{dup.Key}' is selected by multiple players. Please choose unique countries.";
                return;
            }

            // All good
            ValidationMessage = "";
        }

        private string BuildSummary()
        {
            if (Players.Count == 0) return "";
            var teamA = new List<string>();
            var teamB = new List<string>();
            for (int i = 0; i < Players.Count; i++)
            {
                var label = $"P{i + 1}";
                if (Players[i].TeamNumber == Players[0].TeamNumber) teamA.Add(label);
                else teamB.Add(label);
            }
            return $"Teams: [{string.Join(", ", teamA)}] vs [{string.Join(", ", teamB)}]";
        }

        private string GetPlayerCountry(int number)
        {
            int idx = number - 1;
            if (idx >= 0 && idx < Players.Count) return Players[idx].SelectedCountry;
            return "Random";
        }

        private void SetPlayerCountry(int number, string value)
        {
            int idx = number - 1;
            if (idx >= 0 && idx < Players.Count)
            {
                Players[idx].SelectedCountry = string.IsNullOrWhiteSpace(value) ? "Random" : value;
            }
        }

        private bool GetIsAlly(int number)
        {
            int idx = number - 1;
            if (idx <= 0 || idx >= Players.Count) return false;
            return Players[idx].TeamNumber == Players[0].TeamNumber;
        }

        private void SetIsAlly(int number, bool value)
        {
            // Team assignment is derived from SelectedMatchup; users change it via the matchup dropdown.
            // This setter exists only to keep PropertyChanged compatibility; it does nothing intentionally.
        }

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed class MatchupOption
    {
        public string Name { get; }
        public int[] TeamAssignments { get; }

        public MatchupOption(string name, int[] teamAssignments)
        {
            Name = name;
            TeamAssignments = teamAssignments;
        }
    }
}