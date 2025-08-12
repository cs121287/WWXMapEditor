using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using WWXMapEditor.Models;
using System.Linq;
using WWXMapEditor.UI.Scaling;

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
        private ResourceDictionary? _stylesResources;
        private double _currentScale = 1.0;

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
        public event EventHandler? ScalingChanged;
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
            InitializeStylesResources();
        }
        #endregion

        #region Properties
        public AppSettings Settings => _settings;
        public double CurrentScale => _currentScale;
        #endregion

        #region Style Resources Initialization
        private void InitializeStylesResources()
        {
            try
            {
                var app = System.Windows.Application.Current;
                if (app != null)
                {
                    _stylesResources = app.Resources.MergedDictionaries
                        .FirstOrDefault(d => d.Source?.OriginalString?.Contains("Styles.xaml") ?? false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing styles resources: {ex.Message}");
            }
        }
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

            // Apply theme and scaling immediately if changed (hot-reload)
            ApplyTheme();
            ApplyUIScaling();
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

                        // Convert UIScaling percentage string to UiScale double if needed (legacy support)
                        if (!string.IsNullOrEmpty(_settings.UIScaling))
                        {
                            string scaleStr = _settings.UIScaling.Replace("%", "");
                            if (double.TryParse(scaleStr, out double scalePercent))
                            {
                                _settings.UiScale = scalePercent / 100.0;
                            }
                        }

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

                // Update UIScaling string from UiScale for legacy UI (keep in sync for compatibility)
                _settings.UIScaling = $"{(int)(_settings.UiScale * 100)}%";

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

                // Update UIScaling string from UiScale for legacy UI (keep in sync for compatibility)
                _settings.UIScaling = $"{(int)(_settings.UiScale * 100)}%";

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

            // Legacy UI scaling sanity (kept for backward-compat UI)
            var validScalings = new[] { "50%", "75%", "100%", "125%", "150%", "175%", "200%" };
            if (!validScalings.Contains(settings.UIScaling))
            {
                settings.UIScaling = "100%";
                isValid = false;
            }

            // Validate UiScale
            if (settings.UiScale < 0.5 || settings.UiScale > 2.0)
            {
                settings.UiScale = 1.0;
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
                        var newTheme = new ResourceDictionary { Source = themeUri };
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
            if (_settings == null) return;

            // Synchronize AppSettings into ScaleService (new advanced scaling)
            var scaleSvc = ScaleService.Instance;
            scaleSvc.ScaleMode = (_settings.ScaleMode ?? "Automatic");
            scaleSvc.CustomFixedScale = _settings.CustomFixedScale.HasValue && _settings.CustomFixedScale.Value > 0
                ? _settings.CustomFixedScale.Value : 1.0;
            scaleSvc.DesignWidth = _settings.DesignWidth > 0 ? _settings.DesignWidth : 1920;
            scaleSvc.DesignHeight = _settings.DesignHeight > 0 ? _settings.DesignHeight : 1080;
            scaleSvc.MinAutoScale = _settings.MinAutoScale > 0 ? _settings.MinAutoScale : 0.75;
            scaleSvc.MaxAutoScale = _settings.MaxAutoScale > 0 ? _settings.MaxAutoScale : 1.65;
            scaleSvc.MinFontScale = _settings.MinFontScale > 0 ? _settings.MinFontScale : 0.85;
            scaleSvc.MaxFontScale = _settings.MaxFontScale > 0 ? _settings.MaxFontScale : 1.8;
            scaleSvc.EnableBreakpointStyling = _settings.UseDensityBreakpoints;

            // Legacy percent compatibility: if ScaleMode is LegacyPercent, drive UiScale from UIScaling string
            if (string.Equals(_settings.ScaleMode, "LegacyPercent", StringComparison.OrdinalIgnoreCase))
            {
                var scaleFactor = _settings.UIScaling.TrimEnd('%');
                if (!double.TryParse(scaleFactor, out double scalePercent)) scalePercent = 100;
                var uiScale = Math.Clamp(scalePercent / 100.0, 0.5, 2.0);
                _settings.UiScale = uiScale;
                scaleSvc.CustomFixedScale = uiScale; // used when CustomFixed or LegacyPercent fallback
            }
            else if (string.Equals(_settings.ScaleMode, "CustomFixed", StringComparison.OrdinalIgnoreCase))
            {
                // Ensure CustomFixedScale is consistent and non-null
                if (!_settings.CustomFixedScale.HasValue || _settings.CustomFixedScale.Value <= 0)
                {
                    _settings.CustomFixedScale = 1.0;
                }
            }

            // Recompute scales
            scaleSvc.Recompute();

            // Update current scale snapshot
            _currentScale = scaleSvc.EffectiveScale;

            if (System.Windows.Application.Current == null)
                return;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Update app resources based on EffectiveScale (UI sizes) and FontScale (text)
                    UpdateScaledResources(scaleSvc.EffectiveScale, scaleSvc.FontScale);

                    // Notify that scaling has changed
                    ScalingChanged?.Invoke(this, EventArgs.Empty);

                    // Force refresh of all windows
                    foreach (Window window in System.Windows.Application.Current.Windows)
                    {
                        window.UpdateLayout();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error applying UI scaling: {ex.Message}");
                }
            });
        }

        private void UpdateScaledResources(double uiScale, double fontScale)
        {
            // Update font sizes (use fontScale)
            UpdateResource("FontSizeSmall", GetBaseValue("BaseFontSizeSmall") * fontScale);
            UpdateResource("FontSizeMedium", GetBaseValue("BaseFontSizeMedium") * fontScale);
            UpdateResource("FontSizeLarge", GetBaseValue("BaseFontSizeLarge") * fontScale);
            UpdateResource("FontSizeXLarge", GetBaseValue("BaseFontSizeXLarge") * fontScale);
            UpdateResource("FontSizeXXLarge", GetBaseValue("BaseFontSizeXXLarge") * fontScale);
            UpdateResource("FontSizeTitle", GetBaseValue("BaseFontSizeTitle") * fontScale);
            UpdateResource("FontSizeHeader", GetBaseValue("BaseFontSizeHeader") * fontScale);
            UpdateResource("FontSizeMenu", GetBaseValue("BaseFontSizeMenu") * fontScale);

            // Update button sizes (use uiScale)
            UpdateResource("ScaledButtonWidth", GetBaseValue("BaseButtonWidth") * uiScale);
            UpdateResource("ScaledButtonHeight", GetBaseValue("BaseButtonHeight") * uiScale);
            UpdateResource("ScaledEditorButtonSize", GetBaseValue("BaseEditorButtonSize") * uiScale);
            UpdateResource("ScaledToolbarButtonSize", GetBaseValue("BaseToolbarButtonSize") * uiScale);
            UpdateResource("ScaledStepCircleSize", GetBaseValue("BaseStepCircleSize") * uiScale);

            // Update control widths (use uiScale)
            UpdateResource("ScaledComboBoxWidth", GetBaseValue("BaseComboBoxWidth") * uiScale);
            UpdateResource("ScaledTextBoxWidth", GetBaseValue("BaseTextBoxWidth") * uiScale);
            UpdateResource("ScaledBrowseButtonWidth", GetBaseValue("BaseBrowseButtonWidth") * uiScale);

            // Update margins and padding (use uiScale)
            UpdateThickness("MarginSmall", 5 * uiScale);
            UpdateThickness("MarginMedium", 10 * uiScale);
            UpdateThickness("MarginLarge", 20 * uiScale);
            UpdateThickness("MarginXLarge", 30 * uiScale);
            UpdateThickness("PaddingSmall", 5 * uiScale);
            UpdateThickness("PaddingMedium", 10 * uiScale);
            UpdateThickness("PaddingLarge", 15 * uiScale);
            UpdateThickness("PaddingXLarge", 30 * uiScale, 25 * uiScale);
            UpdateThickness("ButtonMargin", 0, 10 * uiScale);
            UpdateThickness("ButtonPadding", 15 * uiScale, 8 * uiScale);
        }

        private double GetBaseValue(string key)
        {
            // First check in styles resources
            if (_stylesResources != null && _stylesResources.Contains(key))
            {
                if (_stylesResources[key] is double value)
                    return value;
            }

            // Then check in application resources
            if (System.Windows.Application.Current?.Resources.Contains(key) == true)
            {
                if (System.Windows.Application.Current.Resources[key] is double value)
                    return value;
            }

            // Return default values if resource not found
            return key switch
            {
                "BaseFontSizeSmall" => 12,
                "BaseFontSizeMedium" => 14,
                "BaseFontSizeLarge" => 16,
                "BaseFontSizeXLarge" => 18,
                "BaseFontSizeXXLarge" => 24,
                "BaseFontSizeTitle" => 32,
                "BaseFontSizeHeader" => 36,
                "BaseFontSizeMenu" => 24,
                "BaseButtonWidth" => 400,
                "BaseButtonHeight" => 60,
                "BaseEditorButtonSize" => 40,
                "BaseToolbarButtonSize" => 36,
                "BaseStepCircleSize" => 30,
                "BaseComboBoxWidth" => 150,
                "BaseTextBoxWidth" => 200,
                "BaseBrowseButtonWidth" => 80,
                _ => 10
            };
        }

        private void UpdateResource(string key, object value)
        {
            if (System.Windows.Application.Current?.Resources != null)
            {
                System.Windows.Application.Current.Resources[key] = value;
            }

            if (_stylesResources != null)
            {
                _stylesResources[key] = value;
            }
        }

        private void UpdateThickness(string key, double uniformValue)
        {
            UpdateResource(key, new Thickness(uniformValue));
        }

        private void UpdateThickness(string key, double horizontal, double vertical)
        {
            UpdateResource(key, new Thickness(horizontal, vertical, horizontal, vertical));
        }

        public double GetUIScale()
        {
            return _settings.UIScaling switch
            {
                "50%" => 0.50,
                "75%" => 0.75,
                "100%" => 1.0,
                "125%" => 1.25,
                "150%" => 1.5,
                "175%" => 1.75,
                "200%" => 2.0,
                _ => 1.0
            };
        }

        public void ApplyAllSettings()
        {
            ApplyTheme();
            ApplyUIScaling();

            // Apply other settings as needed
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

        #region UI Scale Helper Methods
        public void SetUIScale(double scale)
        {
            _settings.UiScale = scale;
            _settings.UIScaling = $"{(int)(scale * 100)}%";
            SaveSettings(_settings);
            ApplyUIScaling();
        }

        public void SetUIScale(string scaleString)
        {
            // Handle percentage strings like "75%", "100%", "125%"
            string cleanScale = scaleString.Replace("%", "").Trim();
            if (double.TryParse(cleanScale, out double scalePercent))
            {
                SetUIScale(scalePercent / 100.0);
            }
        }

        public string GetUIScaleString()
        {
            return $"{(int)(_settings.UiScale * 100)}%";
        }

        public void ToggleTheme()
        {
            _settings.Theme = _settings.Theme == "Dark" ? "Light" : "Dark";
            SaveSettings(_settings);
            ApplyTheme();
        }
        #endregion

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