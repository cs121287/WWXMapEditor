using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WWXMapEditor.ViewModels
{
    public class PlayerSlotViewModel : INotifyPropertyChanged
    {
        private string _selectedCountry = "Random";
        private int _teamNumber = 1;

        public int Index { get; }
        public string DisplayName => $"Player {Index}";

        public ObservableCollection<string> AvailableCountries { get; } = new ObservableCollection<string>();

        public string SelectedCountry
        {
            get => _selectedCountry;
            set
            {
                if (_selectedCountry != value)
                {
                    _selectedCountry = value ?? "Random";
                    OnPropertyChanged(nameof(SelectedCountry));
                }
            }
        }

        public int TeamNumber
        {
            get => _teamNumber;
            set
            {
                if (_teamNumber != value)
                {
                    _teamNumber = value;
                    OnPropertyChanged(nameof(TeamNumber));
                }
            }
        }

        public PlayerSlotViewModel(int index)
        {
            Index = index;
            // Default available countries (will be replaced by the host VM)
            ReplaceAvailableWith(new List<string> { "Random" });
        }

        public void ReplaceAvailableWith(IList<string> items)
        {
            AvailableCountries.Clear();
            foreach (var it in items)
                AvailableCountries.Add(it);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}