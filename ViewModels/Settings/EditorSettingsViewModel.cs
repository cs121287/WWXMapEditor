using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace WWXMapEditor.ViewModels.Settings
{
    public class EditorSettingsViewModel : SettingsPageViewModelBase
    {
        private bool _showGrid = true;
        private SolidColorBrush _gridColor = new SolidColorBrush(Colors.Gray);
        private int _gridOpacity = 50;
        private bool _snapToGrid = true;
        private int _gridSize = 32;
        private int _defaultMapWidth = 50;
        private int _defaultMapHeight = 50;
        private SolidColorBrush _canvasBackgroundColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 51, 51));
        private bool _showTileCoordinates;
        private bool _showRulers = true;
        private bool _hardwareAcceleration = true;
        private string _undoHistoryLimit = "100";
        private string _textureQuality = "High";
        private bool _smoothZooming = true;

        public bool ShowGrid
        {
            get => _showGrid;
            set => SetProperty(ref _showGrid, value);
        }

        public SolidColorBrush GridColor
        {
            get => _gridColor;
            set => SetProperty(ref _gridColor, value);
        }

        public int GridOpacity
        {
            get => _gridOpacity;
            set => SetProperty(ref _gridOpacity, value);
        }

        public bool SnapToGrid
        {
            get => _snapToGrid;
            set => SetProperty(ref _snapToGrid, value);
        }

        public int GridSize
        {
            get => _gridSize;
            set => SetProperty(ref _gridSize, value);
        }

        public int DefaultMapWidth
        {
            get => _defaultMapWidth;
            set => SetProperty(ref _defaultMapWidth, value);
        }

        public int DefaultMapHeight
        {
            get => _defaultMapHeight;
            set => SetProperty(ref _defaultMapHeight, value);
        }

        public SolidColorBrush CanvasBackgroundColor
        {
            get => _canvasBackgroundColor;
            set => SetProperty(ref _canvasBackgroundColor, value);
        }

        public bool ShowTileCoordinates
        {
            get => _showTileCoordinates;
            set => SetProperty(ref _showTileCoordinates, value);
        }

        public bool ShowRulers
        {
            get => _showRulers;
            set => SetProperty(ref _showRulers, value);
        }

        public bool HardwareAcceleration
        {
            get => _hardwareAcceleration;
            set => SetProperty(ref _hardwareAcceleration, value);
        }

        public string UndoHistoryLimit
        {
            get => _undoHistoryLimit;
            set => SetProperty(ref _undoHistoryLimit, value);
        }

        public string TextureQuality
        {
            get => _textureQuality;
            set => SetProperty(ref _textureQuality, value);
        }

        public bool SmoothZooming
        {
            get => _smoothZooming;
            set => SetProperty(ref _smoothZooming, value);
        }

        public ObservableCollection<int> GridSizes { get; }
        public ObservableCollection<string> UndoHistoryLimits { get; }
        public ObservableCollection<string> TextureQualities { get; }

        public ICommand PickGridColorCommand { get; }
        public ICommand PickCanvasColorCommand { get; }

        public EditorSettingsViewModel()
        {
            GridSizes = new ObservableCollection<int> { 16, 32, 64, 128 };
            UndoHistoryLimits = new ObservableCollection<string> { "50", "100", "200", "Unlimited" };
            TextureQualities = new ObservableCollection<string> { "Low", "Medium", "High" };

            PickGridColorCommand = new RelayCommand(ExecutePickGridColor);
            PickCanvasColorCommand = new RelayCommand(ExecutePickCanvasColor);
        }

        private void ExecutePickGridColor(object parameter)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                GridColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    colorDialog.Color.A,
                    colorDialog.Color.R,
                    colorDialog.Color.G,
                    colorDialog.Color.B));
            }
        }

        private void ExecutePickCanvasColor(object parameter)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CanvasBackgroundColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    colorDialog.Color.A,
                    colorDialog.Color.R,
                    colorDialog.Color.G,
                    colorDialog.Color.B));
            }
        }

        public override void LoadSettings()
        {
            // TODO: Load settings from configuration
        }

        public override void SaveSettings()
        {
            // TODO: Save settings to configuration
        }

        public override void ResetToDefaults()
        {
            ShowGrid = true;
            GridColor = new SolidColorBrush(Colors.Gray);
            GridOpacity = 50;
            SnapToGrid = true;
            GridSize = 32;
            DefaultMapWidth = 50;
            DefaultMapHeight = 50;
            CanvasBackgroundColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 51, 51));
            ShowTileCoordinates = false;
            ShowRulers = true;
            HardwareAcceleration = true;
            UndoHistoryLimit = "100";
            TextureQuality = "High";
            SmoothZooming = true;
        }
    }
}