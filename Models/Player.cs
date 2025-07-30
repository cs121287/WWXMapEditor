using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WwXMapEditor.Models
{
    public class Player : INotifyPropertyChanged
    {
        private string _name = "Player";
        private string _country = "Unspecified";
        private bool _isAI;
        private string _color = "Blue";

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Country
        {
            get => _country;
            set
            {
                if (_country != value)
                {
                    _country = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsAI
        {
            get => _isAI;
            set
            {
                if (_isAI != value)
                {
                    _isAI = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}