using System.Collections.ObjectModel;
using System.Windows.Input;

namespace WWXMapEditor.ViewModels.Settings
{
    public class AdvancedSettingsViewModel : SettingsPageViewModelBase
    {
        private bool _showFPSCounter;
        private bool _showMemoryUsage;
        private bool _enableDebugConsole;
        private string _logLevel = "Warning";
        private bool _enablePlugins;
        private string _pluginDirectory = @"C:\Users\{User}\Documents\WWXMapEditor\Plugins";

        public bool ShowFPSCounter
        {
            get => _showFPSCounter;
            set => SetProperty(ref _showFPSCounter, value);
        }

        public bool ShowMemoryUsage
        {
            get => _showMemoryUsage;
            set => SetProperty(ref _showMemoryUsage, value);
        }

        public bool EnableDebugConsole
        {
            get => _enableDebugConsole;
            set => SetProperty(ref _enableDebugConsole, value);
        }

        public string LogLevel
        {
            get => _logLevel;
            set => SetProperty(ref _logLevel, value);
        }

        public bool EnablePlugins
        {
            get => _enablePlugins;
            set => SetProperty(ref _enablePlugins, value);
        }

        public string PluginDirectory
        {
            get => _pluginDirectory;
            set => SetProperty(ref _pluginDirectory, value);
        }

        public ObservableCollection<string> LogLevels { get; }
        public ObservableCollection<PluginInfo> InstalledPlugins { get; }

        public ICommand BrowsePluginDirectoryCommand { get; }

        public AdvancedSettingsViewModel()
        {
            LogLevels = new ObservableCollection<string> { "Error", "Warning", "Info", "Debug" };
            InstalledPlugins = new ObservableCollection<PluginInfo>
            {
                new PluginInfo { Name = "Example Plugin", Description = "A sample plugin for demonstration", Version = "1.0.0", IsEnabled = true },
                new PluginInfo { Name = "Advanced Tools", Description = "Additional tools for map editing", Version = "2.1.0", IsEnabled = false }
            };

            BrowsePluginDirectoryCommand = new RelayCommand(ExecuteBrowsePluginDirectory);
        }

        private void ExecuteBrowsePluginDirectory(object parameter)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PluginDirectory = folderDialog.SelectedPath;
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
            ShowFPSCounter = false;
            ShowMemoryUsage = false;
            EnableDebugConsole = false;
            LogLevel = "Warning";
            EnablePlugins = false;
            PluginDirectory = @"C:\Users\{User}\Documents\WWXMapEditor\Plugins";
        }
    }

    public class PluginInfo : ViewModelBase
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _version = string.Empty;
        private bool _isEnabled;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value ?? string.Empty);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value ?? string.Empty);
        }

        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value ?? string.Empty);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
    }
}