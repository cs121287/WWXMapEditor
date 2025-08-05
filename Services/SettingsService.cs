using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;
using WWXMapEditor.Models;

namespace WWXMapEditor.Services
{
    public class SettingsService
    {
        private static SettingsService _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private readonly string _settingsPath;
        private AppSettings _settings;

        public AppSettings Settings => _settings ??= LoadSettings();

        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        private SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "WWXMapEditor");

            try
            {
                Directory.CreateDirectory(appFolder);
                _settingsPath = Path.Combine(appFolder, "settings.json");
            }
            catch (UnauthorizedAccessException)
            {
                // Fall back to using local folder if we can't create in AppData
                var localFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".";
                appFolder = Path.Combine(localFolder, "Settings");
                Directory.CreateDirectory(appFolder);
                _settingsPath = Path.Combine(appFolder, "settings.json");
            }
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonConvert.DeserializeObject<AppSettings>(json);

                    // Validate settings
                    if (settings != null && ValidateSettings(settings))
                    {
                        settings.IsFirstRun = false;
                        _settings = settings;
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }

            // Return default settings if file doesn't exist or is invalid
            return new AppSettings { IsFirstRun = true };
        }

        public async Task<bool> SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                settings.LastModified = DateTime.UtcNow;
                settings.IsFirstRun = false;

                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                await File.WriteAllTextAsync(_settingsPath, json);

                _settings = settings;
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(settings));

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }

        public bool SaveSettings(AppSettings settings)
        {
            return SaveSettingsAsync(settings).GetAwaiter().GetResult();
        }

        private bool ValidateSettings(AppSettings settings)
        {
            // Perform validation to ensure settings are valid
            if (settings == null) return false;

            // Check required fields
            if (string.IsNullOrEmpty(settings.Theme)) return false;
            if (string.IsNullOrEmpty(settings.Language)) return false;
            if (settings.AutoSaveInterval < 1) return false;
            if (settings.GridSize < 8 || settings.GridSize > 256) return false;
            if (settings.DefaultMapWidth < 1 || settings.DefaultMapHeight < 1) return false;
            if (settings.MouseSensitivity <= 0) return false;

            // Validate paths exist or can be created
            var paths = new[]
            {
                settings.AutoSaveLocation,
                settings.DefaultProjectDirectory,
                settings.DefaultTilesetDirectory,
                settings.DefaultExportDirectory,
                settings.TemplatesDirectory,
                settings.PluginDirectory
            };

            foreach (var path in paths)
            {
                try
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        public void ResetToDefaults()
        {
            _settings = new AppSettings { IsFirstRun = false };
            SaveSettings(_settings);
        }

        public void ApplyTheme()
        {
            if (_settings == null) return;

            switch (_settings.Theme)
            {
                case "Light":
                    ThemeService.Instance.SetTheme(ThemeService.Theme.Light);
                    break;
                case "Dark":
                    ThemeService.Instance.SetTheme(ThemeService.Theme.Dark);
                    break;
                case "Custom":
                    if (System.Windows.Media.ColorConverter.ConvertFromString(_settings.CustomThemeColor) is System.Windows.Media.Color color)
                    {
                        ThemeService.Instance.SetCustomTheme(color);
                    }
                    break;
            }
        }

        public void ApplyUIScaling()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (_settings == null || System.Windows.Application.Current.MainWindow == null) return;

                var scaleFactor = _settings.UIScaling.TrimEnd('%');
                if (double.TryParse(scaleFactor, out double scale))
                {
                    var scaleValue = scale / 100.0;
                    var transform = new ScaleTransform(scaleValue, scaleValue);

                    // Apply transform to the window's content, not the window itself
                    var mainWindow = System.Windows.Application.Current.MainWindow;
                    if (mainWindow.Content is FrameworkElement content)
                    {
                        content.LayoutTransform = transform;

                        // Force layout update
                        mainWindow.UpdateLayout();
                    }
                }
            });
        }

        public void ApplyAllSettings()
        {
            ApplyTheme();
            ApplyUIScaling();
        }
    }

    public class SettingsChangedEventArgs : EventArgs
    {
        public AppSettings NewSettings { get; }

        public SettingsChangedEventArgs(AppSettings newSettings)
        {
            NewSettings = newSettings;
        }
    }
}