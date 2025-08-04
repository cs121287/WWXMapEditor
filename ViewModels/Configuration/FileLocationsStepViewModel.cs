using System.Windows.Input;
using WWXMapEditor.Models;

namespace WWXMapEditor.ViewModels
{
    public class FileLocationsStepViewModel : ViewModelBase
    {
        private readonly AppSettings _settings;
        private string _defaultProjectDirectory = string.Empty;
        private string _defaultTilesetDirectory = string.Empty;
        private string _autoSaveLocation = string.Empty;

        public string DefaultProjectDirectory
        {
            get => _defaultProjectDirectory;
            set => SetProperty(ref _defaultProjectDirectory, value);
        }

        public string DefaultTilesetDirectory
        {
            get => _defaultTilesetDirectory;
            set => SetProperty(ref _defaultTilesetDirectory, value);
        }

        public string AutoSaveLocation
        {
            get => _autoSaveLocation;
            set => SetProperty(ref _autoSaveLocation, value);
        }

        public ICommand BrowseProjectDirectoryCommand { get; }
        public ICommand BrowseTilesetDirectoryCommand { get; }
        public ICommand BrowseAutoSaveLocationCommand { get; }

        public FileLocationsStepViewModel(AppSettings settings)
        {
            _settings = settings;

            // Load from settings
            DefaultProjectDirectory = _settings.DefaultProjectDirectory;
            DefaultTilesetDirectory = _settings.DefaultTilesetDirectory;
            AutoSaveLocation = _settings.AutoSaveLocation;

            BrowseProjectDirectoryCommand = new RelayCommand(ExecuteBrowseProjectDirectory);
            BrowseTilesetDirectoryCommand = new RelayCommand(ExecuteBrowseTilesetDirectory);
            BrowseAutoSaveLocationCommand = new RelayCommand(ExecuteBrowseAutoSaveLocation);
        }

        private void ExecuteBrowseProjectDirectory(object parameter)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DefaultProjectDirectory = dialog.SelectedPath;
            }
        }

        private void ExecuteBrowseTilesetDirectory(object parameter)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DefaultTilesetDirectory = dialog.SelectedPath;
            }
        }

        private void ExecuteBrowseAutoSaveLocation(object parameter)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AutoSaveLocation = dialog.SelectedPath;
            }
        }

        public void UpdateSettings()
        {
            _settings.DefaultProjectDirectory = DefaultProjectDirectory;
            _settings.DefaultTilesetDirectory = DefaultTilesetDirectory;
            _settings.AutoSaveLocation = AutoSaveLocation;
        }
    }
}