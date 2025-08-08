using System;
using System.Windows.Input;
using WWXMapEditor.Services;
using WWXMapEditor.Models;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Linq;

namespace WWXMapEditor.ViewModels
{
    public class MainMenuViewModel : ViewModelBase, IDisposable
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private string _currentTime = DateTime.Now.ToString("HH:mm");
        private System.Windows.Threading.DispatcherTimer? _timer;
        private bool _disposed = false;

        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        // Collections for recent maps
        public ObservableCollection<RecentMap> RecentMaps { get; }

        // Commands
        public ICommand CreateNewMapCommand { get; }
        public ICommand OpenMapCommand { get; }
        public ICommand OpenRecentMapCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand ExitCommand { get; }

        // Legacy command names for compatibility
        public ICommand NewMapCommand => CreateNewMapCommand;
        public ICommand LoadMapCommand => OpenMapCommand;

        public MainMenuViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;

            // Initialize collections
            RecentMaps = new ObservableCollection<RecentMap>();
            LoadRecentMaps();

            // Initialize commands
            CreateNewMapCommand = new RelayCommand(ExecuteCreateNewMap);
            OpenMapCommand = new RelayCommand(ExecuteOpenMap);
            OpenRecentMapCommand = new RelayCommand(ExecuteOpenRecentMap);
            SettingsCommand = new RelayCommand(ExecuteSettings);
            AboutCommand = new RelayCommand(ExecuteAbout);
            ExitCommand = new RelayCommand(ExecuteExit);

            // Setup timer for clock
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm");
        }

        private void LoadRecentMaps()
        {
            try
            {
                RecentMaps.Clear();

                // Load from settings service
                var settings = SettingsService.Instance.Settings;
                if (settings.RecentMaps != null && settings.RecentMaps.Any())
                {
                    foreach (var recentMapPath in settings.RecentMaps.Take(5))
                    {
                        if (!string.IsNullOrEmpty(recentMapPath) && System.IO.File.Exists(recentMapPath))
                        {
                            try
                            {
                                var fileInfo = new System.IO.FileInfo(recentMapPath);
                                var mapService = new MapService();
                                var result = mapService.LoadMap(recentMapPath);

                                if (result.Success && result.Data != null)
                                {
                                    RecentMaps.Add(new RecentMap
                                    {
                                        Name = result.Data.Name,
                                        FilePath = recentMapPath,
                                        Size = $"{result.Data.Width}x{result.Data.Height}",
                                        LastModified = GetRelativeTime(fileInfo.LastWriteTime)
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error loading recent map {recentMapPath}: {ex.Message}");
                            }
                        }
                    }
                }

                // Add placeholder if no recent maps
                if (RecentMaps.Count == 0)
                {
                    RecentMaps.Add(new RecentMap
                    {
                        Name = "No recent maps",
                        LastModified = "",
                        Size = "",
                        FilePath = ""
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recent maps: {ex.Message}");

                // Ensure at least placeholder exists
                if (RecentMaps.Count == 0)
                {
                    RecentMaps.Add(new RecentMap
                    {
                        Name = "No recent maps",
                        LastModified = "",
                        Size = "",
                        FilePath = ""
                    });
                }
            }
        }

        private string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) != 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) != 1 ? "s" : "")} ago";

            return dateTime.ToShortDateString();
        }

        private void ExecuteCreateNewMap(object? parameter)
        {
            Cleanup();
            _mainWindowViewModel.NavigateToNewMapCreation();
        }

        private void ExecuteOpenMap(object? parameter)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "WWX Map Files (*.wwxmap)|*.wwxmap|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                    DefaultExt = ".wwxmap",
                    Title = "Open Map",
                    InitialDirectory = SettingsService.Instance.Settings.DefaultProjectDirectory
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var mapService = new MapService();
                    var result = mapService.LoadMap(openFileDialog.FileName);

                    if (result.Success && result.Data != null)
                    {
                        AddToRecentMaps(result.Data, openFileDialog.FileName);
                        Cleanup();
                        _mainWindowViewModel.NavigateToMapEditor(result.Data);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            $"Failed to load map file:\n{result.ErrorMessage}",
                            "Error Loading Map",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"An unexpected error occurred:\n{ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void ExecuteOpenRecentMap(object? parameter)
        {
            try
            {
                if (parameter is RecentMap recentMap && !string.IsNullOrEmpty(recentMap.FilePath))
                {
                    // Ignore placeholder
                    if (recentMap.FilePath == "" && recentMap.Name == "No recent maps")
                    {
                        return;
                    }

                    var mapService = new MapService();
                    var result = mapService.LoadMap(recentMap.FilePath);

                    if (result.Success && result.Data != null)
                    {
                        AddToRecentMaps(result.Data, recentMap.FilePath);
                        Cleanup();
                        _mainWindowViewModel.NavigateToMapEditor(result.Data);
                    }
                    else
                    {
                        // Remove from recent maps if file no longer exists or is corrupted
                        var settings = SettingsService.Instance.Settings;
                        settings.RecentMaps.Remove(recentMap.FilePath);
                        SettingsService.Instance.SaveSettings(settings);

                        // Reload recent maps list
                        LoadRecentMaps();

                        System.Windows.MessageBox.Show(
                            $"Cannot open map:\n{result.ErrorMessage}",
                            "Error",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"An unexpected error occurred:\n{ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void AddToRecentMaps(Map map, string filePath)
        {
            try
            {
                var settings = SettingsService.Instance.Settings;

                // Remove if already exists
                if (settings.RecentMaps.Contains(filePath))
                {
                    settings.RecentMaps.Remove(filePath);
                }

                // Add to beginning of list
                settings.RecentMaps.Insert(0, filePath);

                // Keep only the configured number of recent maps
                while (settings.RecentMaps.Count > settings.RecentFilesCount)
                {
                    settings.RecentMaps.RemoveAt(settings.RecentMaps.Count - 1);
                }

                // Save settings
                SettingsService.Instance.SaveSettings(settings);

                // Reload the UI list
                LoadRecentMaps();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding to recent maps: {ex.Message}");
            }
        }

        private void ExecuteSettings(object? parameter)
        {
            Cleanup();
            _mainWindowViewModel.NavigateToSettings();
        }

        private void ExecuteAbout(object? parameter)
        {
            Cleanup();
            _mainWindowViewModel.NavigateToAbout();
        }

        private void ExecuteExit(object? parameter)
        {
            Cleanup();
            System.Windows.Application.Current.Shutdown();
        }

        public void Cleanup()
        {
            if (!_disposed)
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Tick -= Timer_Tick;
                    _timer = null;
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}