using System.IO;
using System.Windows;
using WWXMapEditor.Services;
using WWXMapEditor.ViewModels;
using WWXMapEditor.Views;

namespace WWXMapEditor
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize settings service
            var settingsService = SettingsService.Instance;

            // Check if settings file exists
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WWXMapEditor",
                "settings.json"
            );

            bool settingsExist = File.Exists(settingsPath);

            // Load settings if they exist
            if (settingsExist)
            {
                var settings = settingsService.LoadSettings();

                // Apply theme and UI scaling globally
                settingsService.ApplyTheme();
                settingsService.ApplyUIScaling();
            }
            else
            {
                // Set default theme for first run
                ThemeService.Instance.SetTheme(ThemeService.Theme.Dark);
            }

            // Create main window
            var mainWindow = new MainWindow();
            var mainViewModel = new MainWindowViewModel();

            mainWindow.DataContext = mainViewModel;

            // Check if this is first run or settings are invalid
            if (!settingsExist || settingsService.Settings.IsFirstRun)
            {
                // Navigate to configuration
                mainViewModel.NavigateToConfiguration();
            }
            else
            {
                // Navigate to main menu
                mainViewModel.NavigateToMainMenu();
            }

            // Show the window after navigation is set
            mainWindow.Show();
        }
    }
}