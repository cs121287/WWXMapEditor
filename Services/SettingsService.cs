using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using WWXMapEditor.Models;

namespace WWXMapEditor.Services
{
    public class SettingsService : INotifyPropertyChanged
    {
        private static SettingsService? _instance;
        private static readonly object _lock = new object();
        private AppSettings _settings;
        private readonly string _settingsPath;
        private readonly ThemeService _themeService;
        private FileSystemWatcher? _fileWatcher;
        private readonly JsonSerializerOptions _serializerOptions;

        #region Singleton Instance
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
        #endregion

        #region Events
        public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region Constructor
        private SettingsService()
        {
            _themeService = ThemeService.Instance;

            // Initialize settings path with proper error handling
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WWXMapEditor");

            try
            {
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create settings directory: {ex.Message}");
                // Fall back to temp directory if we can't create app folder
                appDataPath = Path.GetTempPath();
            }

            _settingsPath = Path.Combine(appDataPath, "settings.json");

            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            _settings = LoadSettings();
            InitializeFileWatcher();
        }
        #endregion

        #region Properties
        public AppSettings Settings => _settings;
        #endregion

        #region File Watcher
        private void InitializeFileWatcher()
        {
            try
            {
                var directory = Path.GetDirectoryName(_settingsPath);
                if (directory != null && Directory.Exists(directory))
                {
                    _fileWatcher = new FileSystemWatcher(directory)
                    {
                        Filter = Path.GetFileName(_settingsPath),
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                    };
                    _fileWatcher.Changed += OnSettingsFileChanged;
                    _fileWatcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize file watcher: {ex.Message}");
            }
        }

        private async void OnSettingsFileChanged(object sender, FileSystemEventArgs e)
        {
            // Debounce file changes
            await Task.Delay(100);

            // Reload settings if file was modified externally
            LoadSettings();

            // Apply theme immediately if changed (hot-reload)
            ApplyTheme();
        }
        #endregion

        #region Load/Save Settings
        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var loadedSettings = JsonSerializer.Deserialize<AppSettings>(json, _serializerOptions);

                    if (loadedSettings != null && ValidateSettings(loadedSettings))
                    {
                        var oldSettings = _settings;
                        _settings = loadedSettings;

                        // Notify about settings change
                        if (oldSettings != null)
                        {
                            OnSettingsChanged("Settings", oldSettings, _settings);
                        }
                    }
                    else
                    {
                        _settings = new AppSettings();
                    }
                }
                else
                {
                    _settings = new AppSettings();
                    SaveSettings(_settings); // Create default settings file
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
                if (!ValidateSettings(settings))
                {
                    System.Diagnostics.Debug.WriteLine("Invalid settings, using defaults for invalid values");
                }

                var oldSettings = _settings;
                _settings = settings;

                // Temporarily disable file watcher to avoid circular updates
                if (_fileWatcher != null)
                    _fileWatcher.EnableRaisingEvents = false;

                // Write to temp file first, then move (atomic operation)
                var tempFile = _settingsPath + ".tmp";
                var json = JsonSerializer.Serialize(settings, _serializerOptions);
                File.WriteAllText(tempFile, json);

                if (File.Exists(_settingsPath))
                {
                    // Create backup
                    var backupPath = _settingsPath + ".backup";
                    File.Replace(tempFile, _settingsPath, backupPath);
                }
                else
                {
                    File.Move(tempFile, _settingsPath);
                }

                // Re-enable file watcher
                if (_fileWatcher != null)
                    _fileWatcher.EnableRaisingEvents = true;

                // Notify about settings change
                OnSettingsChanged("Settings", oldSettings, _settings);
            }
            catch (Exception ex)
            {
                // Log error or handle it appropriately
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");

                // Re-enable file watcher even on error
                if (_fileWatcher != null)
                    _fileWatcher.EnableRaisingEvents = true;

                throw;
            }
        }

        public async Task<bool> SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                if (!ValidateSettings(settings))
                {
                    System.Diagnostics.Debug.WriteLine("Invalid settings, using defaults for invalid values");
                }

                var oldSettings = _settings;
                _settings = settings;

                // Temporarily disable file watcher to avoid circular updates
                if (_fileWatcher != null)
                    _fileWatcher.EnableRaisingEvents = false;

                var json = JsonSerializer.Serialize(settings, _serializerOptions);

                // Write to temp file first, then move (atomic operation)
                var tempFile = _settingsPath + ".tmp";
                await File.WriteAllTextAsync(tempFile, json);

                if (File.Exists(_settingsPath))
                {
                    // Create backup
                    var backupPath = _settingsPath + ".backup";
                    File.Replace(tempFile, _settingsPath, backupPath);
                }
                else
                {
                    File.Move(tempFile, _settingsPath);
                }

                // Re-enable file watcher
                if (_fileWatcher != null)
                    _fileWatcher.EnableRaisingEvents = true;

                // Notify about settings change
                OnSettingsChanged("Settings", oldSettings, _settings);

                return true;
            }
            catch (Exception ex)
            {
                // Log error or handle it appropriately
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");

                // Re-enable file watcher even on error
                if (_fileWatcher != null)
                    _fileWatcher.EnableRaisingEvents = true;

                return false;
            }
        }
        #endregion

        #region Settings Validation
        public bool ValidateSettings(AppSettings settings)
        {
            if (settings == null) return false;

            bool isValid = true;

            // Validate grid size
            if (settings.GridSize < 16 || settings.GridSize > 128)
            {
                settings.GridSize = 32;
                isValid = false;
            }

            // Validate auto-save interval
            if (settings.AutoSaveInterval < 1 || settings.AutoSaveInterval > 60)
            {
                settings.AutoSaveInterval = 5;
                isValid = false;
            }

            // Validate recent files count
            if (settings.RecentFilesCount < 1 || settings.RecentFilesCount > 50)
            {
                settings.RecentFilesCount = 10;
                isValid = false;
            }

            // Validate theme
            if (string.IsNullOrEmpty(settings.Theme) ||
                (settings.Theme != "Dark" && settings.Theme != "Light"))
            {
                settings.Theme = "Dark";
                isValid = false;
            }

            // Validate UI scaling
            var validScalings = new[] { "75%", "100%", "125%", "150%", "175%", "200%" };
            if (!validScalings.Contains(settings.UIScaling))
            {
                settings.UIScaling = "100%";
                isValid = false;
            }

            // Ensure lists are not null
            settings.RecentFiles ??= new System.Collections.Generic.List<string>();
            settings.RecentMaps ??= new System.Collections.Generic.List<string>();
            settings.KeyboardShortcuts ??= new System.Collections.Generic.Dictionary<string, string>();

            return isValid;
        }
        #endregion

        #region Apply Settings
        public void ApplyTheme()
        {
            try
            {
                // Use existing ThemeService for backward compatibility
                var theme = _settings.Theme == "Light" ? ThemeService.Theme.Light : ThemeService.Theme.Dark;
                _themeService.SetTheme(theme);

                // Also support hot-reload with new theme system
                var app = System.Windows.Application.Current;
                if (app == null) return;

                app.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // Check if we have theme resources
                    var themeUri = _settings.Theme?.ToLower() switch
                    {
                        "dark" => new Uri("/Resources/Themes/DarkTheme.xaml", UriKind.Relative),
                        "light" => new Uri("/Resources/Themes/LightTheme.xaml", UriKind.Relative),
                        _ => new Uri("/Resources/Themes/DarkTheme.xaml", UriKind.Relative)
                    };

                    try
                    {
                        // Remove existing theme
                        var existingTheme = app.Resources.MergedDictionaries
                            .FirstOrDefault(d => d.Source?.OriginalString?.Contains("Theme.xaml") ?? false);

                        if (existingTheme != null)
                        {
                            app.Resources.MergedDictionaries.Remove(existingTheme);
                        }

                        // Add new theme
                        var newTheme = new System.Windows.ResourceDictionary { Source = themeUri };
                        app.Resources.MergedDictionaries.Add(newTheme);
                    }
                    catch
                    {
                        // If new theme system fails, existing ThemeService is already applied
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
            }
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
                "175%" => 1.75,
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
                "175%" => 1.75,
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
            OnPropertyChanged(nameof(Settings));
        }

        public void ResetToDefaults()
        {
            var oldSettings = _settings;
            _settings = new AppSettings();
            SaveSettings(_settings);
            ApplyAllSettings();
            OnSettingsChanged("Settings", oldSettings, _settings);
        }
        #endregion

        #region Property Changed
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnSettingsChanged(string propertyName, object? oldValue, object? newValue)
        {
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs
            {
                PropertyName = propertyName,
                OldValue = oldValue,
                NewValue = newValue
            });
        }
        #endregion
    }

    #region Event Args
    public class SettingsChangedEventArgs : EventArgs
    {
        public string PropertyName { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }
    #endregion
}