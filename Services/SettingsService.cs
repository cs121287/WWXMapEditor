using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WWXMapEditor.Models;

namespace WWXMapEditor.Services
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        private static readonly object _lock = new object();
        private AppSettings _settings;
        private readonly string _settingsPath;
        private readonly ThemeService _themeService;

        public static SettingsService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SettingsService();
                        }
                    }
                }
                return _instance;
            }
        }

        private SettingsService()
        {
            _themeService = ThemeService.Instance;
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WWXMapEditor");
            Directory.CreateDirectory(appDataPath);
            _settingsPath = Path.Combine(appDataPath, "settings.json");
            _settings = LoadSettings();
        }

        public AppSettings Settings => _settings;

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    _settings = new AppSettings();
                }
            }
            catch (Exception ex)
            {
                // Log error or handle it appropriately
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                _settings = new AppSettings();
            }

            return _settings;
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                _settings = settings;
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                // Log error or handle it appropriately
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                _settings = settings;
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_settingsPath, json);
                return true;
            }
            catch (Exception ex)
            {
                // Log error or handle it appropriately
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }

        public void ApplyTheme()
        {
            var theme = _settings.Theme == "Light" ? ThemeService.Theme.Light : ThemeService.Theme.Dark;
            _themeService.SetTheme(theme);
        }

        public void ApplyUIScaling()
        {
            if (System.Windows.Application.Current?.MainWindow == null) return;

            double scale = _settings.UIScaling switch
            {
                "75%" => 0.75,
                "100%" => 1.0,
                "125%" => 1.25,
                "150%" => 1.5,
                "200%" => 2.0,
                _ => 1.0
            };

            // Apply scaling to all windows in the application
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                ApplyScalingToWindow(window, scale);
            }

            // Set up handler for new windows
            System.Windows.Application.Current.Activated -= OnApplicationActivated;
            System.Windows.Application.Current.Activated += OnApplicationActivated;
        }

        private void OnApplicationActivated(object? sender, EventArgs e)
        {
            // Apply scaling to any new windows
            double scale = _settings.UIScaling switch
            {
                "75%" => 0.75,
                "100%" => 1.0,
                "125%" => 1.25,
                "150%" => 1.5,
                "200%" => 2.0,
                _ => 1.0
            };

            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window.Tag?.ToString() != "Scaled")
                {
                    ApplyScalingToWindow(window, scale);
                    window.Tag = "Scaled";
                }
            }
        }

        private void ApplyScalingToWindow(Window window, double scale)
        {
            var transform = new ScaleTransform(scale, scale);
            window.LayoutTransform = transform;

            // Adjust window size to compensate for scaling
            if (window.WindowState == WindowState.Normal)
            {
                window.Width = window.ActualWidth / scale;
                window.Height = window.ActualHeight / scale;
            }
        }

        public void ApplyAllSettings()
        {
            ApplyTheme();
            ApplyUIScaling();

            // Apply other settings as needed
            // For example, you might want to notify other parts of the application
            // about settings changes
        }

        public void ResetToDefaults()
        {
            _settings = new AppSettings();
            SaveSettings(_settings);
            ApplyAllSettings();
        }
    }
}