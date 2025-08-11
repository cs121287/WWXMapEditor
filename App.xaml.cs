using System;
using System.IO;
using System.Windows;
using WWXMapEditor.Services;
using WWXMapEditor.Views;
using WWXMapEditor.ViewModels;

namespace WWXMapEditor
{
    public partial class App : System.Windows.Application
    {
        private SettingsService? _settingsService;
        private bool _isFirstRun = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize settings service first
            _settingsService = SettingsService.Instance;

            // Check if settings exist and are valid
            bool settingsValid = CheckSettings();

            // Apply theme first
            _settingsService.ApplyTheme();

            // Apply UI scaling immediately after theme
            _settingsService.ApplyUIScaling();

            // Create main window but don't show it yet
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;

            if (!settingsValid || _isFirstRun)
            {
                // Show configuration window first
                var configWindow = new Window
                {
                    Title = "WWX Map Editor - Initial Configuration",
                    Width = 1200,
                    Height = 800,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = (System.Windows.Media.Brush)FindResource("BackgroundBrush")
                };

                var configView = new ConfigurationView();
                var mainViewModel = new MainWindowViewModel();
                var configViewModel = new ConfigurationViewModel(mainViewModel)
                {
                    IsInitialSetup = true
                };
                configView.DataContext = configViewModel;
                configWindow.Content = configView;

                // Handle configuration window closing
                configWindow.Closed += (s, args) =>
                {
                    // Apply all settings including scaling
                    _settingsService.ApplyAllSettings();
                    mainWindow.Show();
                };

                configWindow.Show();
            }
            else
            {
                // Settings are valid, show main window directly
                mainWindow.Show();
            }
        }

        private bool CheckSettings()
        {
            try
            {
                var settingsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "WWXMapEditor",
                    "settings.json"
                );

                if (!File.Exists(settingsPath))
                {
                    _isFirstRun = true;
                    return false;
                }

                // Load and validate settings
                var settings = _settingsService.LoadSettings();

                // Check if this is marked as first run
                if (settings.IsFirstRun)
                {
                    _isFirstRun = true;
                    settings.IsFirstRun = false;
                    _settingsService.SaveSettings(settings);
                    return false;
                }

                // Validate critical settings
                bool isValid = _settingsService.ValidateSettings(settings);
                return isValid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking settings: {ex.Message}");
                return false;
            }
        }
    }
}