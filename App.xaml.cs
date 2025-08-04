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

            // Load settings
            var settings = SettingsService.Instance.Settings;

            // Create main window
            var mainWindow = new MainWindow();
            var mainViewModel = new MainWindowViewModel();

            mainWindow.DataContext = mainViewModel;

            // Check if this is first run or settings are invalid
            if (settings.IsFirstRun)
            {
                // Apply default dark theme for configuration
                ThemeService.Instance.SetTheme(ThemeService.Theme.Dark);

                // Navigate to configuration
                mainViewModel.NavigateToConfiguration();
            }
            else
            {
                // Apply saved theme
                SettingsService.Instance.ApplyTheme();

                // Navigate to main menu
                mainViewModel.NavigateToMainMenu();
            }

            // Show the window after navigation is set
            mainWindow.Show();

            // Apply UI scaling if not first run
            if (!settings.IsFirstRun)
            {
                SettingsService.Instance.ApplyUIScaling();
            }
        }
    }
}