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

            // Make settings globally bindable by views (Application.Resources)
            TryPublishSettingsGlobals();

            // Apply theme first
            _settingsService.ApplyTheme();

            // Apply UI scaling immediately after theme
            _settingsService.ApplyUIScaling();

            // Create main window but don't show it yet
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;

            if (!settingsValid || _isFirstRun)
            {
                // Show configuration window first; block until done so settings are saved/loaded before main window shows
                var configWindow = new Window
                {
                    Title = "WWX Map Editor - Initial Configuration",
                    Width = 1200,
                    Height = 800,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = (System.Windows.Media.Brush)FindResource("BackgroundBrush"),
                    ShowInTaskbar = true,
                    ResizeMode = ResizeMode.CanResize
                };

                var configView = new ConfigurationView();
                var mainViewModel = new MainWindowViewModel();
                var configViewModel = new ConfigurationViewModel(mainViewModel)
                {
                    IsInitialSetup = true
                };
                configView.DataContext = configViewModel;
                configWindow.Content = configView;

                // When closed, apply all settings again and show main window
                configWindow.Closed += (s, args) =>
                {
                    // Ensure latest settings are loaded from disk
                    _settingsService.LoadSettings();

                    // Apply all settings including theme and scaling
                    _settingsService.ApplyAllSettings();

                    // Re-publish settings globally for any bindings
                    TryPublishSettingsGlobals();

                    // Finally show main window
                    mainWindow.Show();
                };

                // Block until configuration is complete
                configWindow.ShowDialog();
            }
            else
            {
                // Settings are valid, show main window directly
                mainWindow.Show();
            }

            // Keep globals in sync if settings change at runtime
            _settingsService.SettingsChanged += (_, __) => TryPublishSettingsGlobals();
        }

        private void TryPublishSettingsGlobals()
        {
            if (_settingsService == null) return;

            Current.Resources["SettingsService"] = _settingsService;
            Current.Resources["AppSettings"] = _settingsService.Settings;
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
                var settings = _settingsService!.LoadSettings();

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