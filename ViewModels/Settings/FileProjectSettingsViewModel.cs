using System.Collections.ObjectModel;
using System.Windows.Input;

namespace WWXMapEditor.ViewModels.Settings
{
    public class FileProjectSettingsViewModel : SettingsPageViewModelBase
    {
        private string _defaultProjectDirectory = @"C:\Users\{User}\Documents\WWXMapEditor\Projects";
        private string _defaultTilesetDirectory = @"C:\Users\{User}\Documents\WWXMapEditor\Tilesets";
        private string _defaultExportDirectory = @"C:\Users\{User}\Documents\WWXMapEditor\Exports";
        private string _templatesDirectory = @"C:\Users\{User}\Documents\WWXMapEditor\Templates";
        private string _defaultSaveFormat = ".wwx";
        private string _compressionLevel = "Fast";
        private bool _includeMetadata = true;
        private bool _backupOnSave = true;

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

        public string DefaultExportDirectory
        {
            get => _defaultExportDirectory;
            set => SetProperty(ref _defaultExportDirectory, value);
        }

        public string TemplatesDirectory
        {
            get => _templatesDirectory;
            set => SetProperty(ref _templatesDirectory, value);
        }

        public string DefaultSaveFormat
        {
            get => _defaultSaveFormat;
            set => SetProperty(ref _defaultSaveFormat, value);
        }

        public string CompressionLevel
        {
            get => _compressionLevel;
            set => SetProperty(ref _compressionLevel, value);
        }

        public bool IncludeMetadata
        {
            get => _includeMetadata;
            set => SetProperty(ref _includeMetadata, value);
        }

        public bool BackupOnSave
        {
            get => _backupOnSave;
            set => SetProperty(ref _backupOnSave, value);
        }

        public ObservableCollection<string> SaveFormats { get; }
        public ObservableCollection<string> CompressionLevels { get; }

        public ICommand BrowseProjectDirectoryCommand { get; }
        public ICommand BrowseTilesetDirectoryCommand { get; }
        public ICommand BrowseExportDirectoryCommand { get; }
        public ICommand BrowseTemplatesDirectoryCommand { get; }

        public FileProjectSettingsViewModel()
        {
            SaveFormats = new ObservableCollection<string> { ".wwx", ".json", ".xml" };
            CompressionLevels = new ObservableCollection<string> { "None", "Fast", "Best" };

            BrowseProjectDirectoryCommand = new RelayCommand(ExecuteBrowseProjectDirectory);
            BrowseTilesetDirectoryCommand = new RelayCommand(ExecuteBrowseTilesetDirectory);
            BrowseExportDirectoryCommand = new RelayCommand(ExecuteBrowseExportDirectory);
            BrowseTemplatesDirectoryCommand = new RelayCommand(ExecuteBrowseTemplatesDirectory);
        }

        private void ExecuteBrowseProjectDirectory(object parameter)
        {
            // TODO: Show folder browser dialog
        }

        private void ExecuteBrowseTilesetDirectory(object parameter)
        {
            // TODO: Show folder browser dialog
        }

        private void ExecuteBrowseExportDirectory(object parameter)
        {
            // TODO: Show folder browser dialog
        }

        private void ExecuteBrowseTemplatesDirectory(object parameter)
        {
            // TODO: Show folder browser dialog
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
            DefaultProjectDirectory = @"C:\Users\{User}\Documents\WWXMapEditor\Projects";
            DefaultTilesetDirectory = @"C:\Users\{User}\Documents\WWXMapEditor\Tilesets";
            DefaultExportDirectory = @"C:\Users\{User}\Documents\WWXMapEditor\Exports";
            TemplatesDirectory = @"C:\Users\{User}\Documents\WWXMapEditor\Templates";
            DefaultSaveFormat = ".wwx";
            CompressionLevel = "Fast";
            IncludeMetadata = true;
            BackupOnSave = true;
        }
    }
}