using System;
using System.ComponentModel;
using System.Reflection;

namespace WWXMapEditor.ViewModels
{
    // ViewModel for the Player Select step.
    // - Exposes ParentViewModel so the view can bind to all player/country fields.
    // - Enforces UNIQUE country selection across players at the ViewModel layer.
    //   "Last writer wins": if a player picks a country already taken, the previous owner is reset to "Random".
    // - Provides an optional inner StepIndex (1..6) if the view chooses to implement a multi-page experience.
    public class PlayerSelectStepViewModel : ViewModelBase, IDisposable
    {
        private readonly NewMapViewModel _parentViewModel;
        private bool _enforcingUniqueness;
        private int _stepIndex = 1;

        public NewMapViewModel ParentViewModel => _parentViewModel;

        // Optional inner step index for a split player-select flow (1..6).
        // Views can bind to this if they paginate inside the Player Select step.
        public int StepIndex
        {
            get => _stepIndex;
            set => SetProperty(ref _stepIndex, value);
        }

        public PlayerSelectStepViewModel(NewMapViewModel parentViewModel)
        {
            _parentViewModel = parentViewModel;

            // Subscribe to detect country changes and enforce uniqueness.
            _parentViewModel.PropertyChanged += OnParentPropertyChanged;
        }

        private void OnParentPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_enforcingUniqueness) return;

            // Only care about country changes
            if (e.PropertyName == nameof(NewMapViewModel.Player1Country) ||
                e.PropertyName == nameof(NewMapViewModel.Player2Country) ||
                e.PropertyName == nameof(NewMapViewModel.Player3Country) ||
                e.PropertyName == nameof(NewMapViewModel.Player4Country))
            {
                EnforceUniqueCountries(e.PropertyName!);
            }
        }

        private void EnforceUniqueCountries(string changedPropertyName)
        {
            try
            {
                _enforcingUniqueness = true;

                string p1 = _parentViewModel.Player1Country ?? "Random";
                string p2 = _parentViewModel.Player2Country ?? "Random";
                string p3 = _parentViewModel.Player3Country ?? "Random";
                string p4 = _parentViewModel.Player4Country ?? "Random";

                // "Last writer wins" strategy:
                // - Determine the value that was just applied (on changedPropertyName).
                // - If that value is non-Random and any other player has the same value,
                //   reset the other player's selection to "Random".
                string changedValue = GetValueByName(changedPropertyName);
                if (IsSpecificCountry(changedValue))
                {
                    if (changedPropertyName != nameof(NewMapViewModel.Player1Country) && p1.Equals(changedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        _parentViewModel.Player1Country = "Random";
                    }
                    if (changedPropertyName != nameof(NewMapViewModel.Player2Country) && p2.Equals(changedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        _parentViewModel.Player2Country = "Random";
                    }
                    if (changedPropertyName != nameof(NewMapViewModel.Player3Country) && p3.Equals(changedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        _parentViewModel.Player3Country = "Random";
                    }
                    if (changedPropertyName != nameof(NewMapViewModel.Player4Country) && p4.Equals(changedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        _parentViewModel.Player4Country = "Random";
                    }
                }

                // Safety: if NumberOfPlayers decreased, ensure "excess" players are reset to Random.
                switch (_parentViewModel.NumberOfPlayers)
                {
                    case 2:
                        if (IsSpecificCountry(_parentViewModel.Player3Country)) _parentViewModel.Player3Country = "Random";
                        if (IsSpecificCountry(_parentViewModel.Player4Country)) _parentViewModel.Player4Country = "Random";
                        break;
                    case 3:
                        if (IsSpecificCountry(_parentViewModel.Player4Country)) _parentViewModel.Player4Country = "Random";
                        break;
                        // case 4: nothing to do
                }
            }
            finally
            {
                _enforcingUniqueness = false;
            }
        }

        private static bool IsSpecificCountry(string? value)
        {
            // Any non-empty value other than "Random" is considered a specific pick.
            return !string.IsNullOrWhiteSpace(value) &&
                   !value.Equals("Random", StringComparison.OrdinalIgnoreCase);
        }

        private string GetValueByName(string propertyName)
        {
            return propertyName switch
            {
                nameof(NewMapViewModel.Player1Country) => _parentViewModel.Player1Country ?? "Random",
                nameof(NewMapViewModel.Player2Country) => _parentViewModel.Player2Country ?? "Random",
                nameof(NewMapViewModel.Player3Country) => _parentViewModel.Player3Country ?? "Random",
                nameof(NewMapViewModel.Player4Country) => _parentViewModel.Player4Country ?? "Random",
                _ => "Random"
            };
        }

        public void Dispose()
        {
            _parentViewModel.PropertyChanged -= OnParentPropertyChanged;
        }
    }
}