using System.Windows.Media;
using WWXMapEditor.ViewModels;

namespace WWXMapEditor.Models
{
    public class TilePaletteItem : ViewModelBase
    {
        private string _name = string.Empty;
        private string _terrainType = string.Empty;
        private System.Windows.Media.Brush _color = System.Windows.Media.Brushes.Gray;
        private bool _isSelected;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string TerrainType
        {
            get => _terrainType;
            set => SetProperty(ref _terrainType, value);
        }

        public System.Windows.Media.Brush Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}