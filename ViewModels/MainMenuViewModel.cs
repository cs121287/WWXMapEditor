using System;
using System.Windows.Input;
using WWXMapEditor.Services;
using WWXMapEditor.Models;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace WWXMapEditor.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private string _currentTime = DateTime.Now.ToString("HH:mm");
        private System.Windows.Threading.DispatcherTimer _timer;

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
            // TODO: Load from saved recent maps
            // For now, add some dummy data
            RecentMaps.Add(new RecentMap { Name = "Battle of the Hills", LastModified = "2 hours ago", Size = "100x100", FilePath = "" });
            RecentMaps.Add(new RecentMap { Name = "Desert Storm", LastModified = "Yesterday", Size = "50x50", FilePath = "" });
            RecentMaps.Add(new RecentMap { Name = "Naval Assault", LastModified = "3 days ago", Size = "150x75", FilePath = "" });
        }

        private void ExecuteCreateNewMap(object? parameter)
        {
            _mainWindowViewModel.NavigateToNewMapCreation();
        }

        private void ExecuteOpenMap(object? parameter)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "WWX Map Files (*.wwxmap)|*.wwxmap|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = ".wwxmap",
                Title = "Open Map"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var mapService = new MapService();
                var map = mapService.LoadMap(openFileDialog.FileName);

                if (map != null)
                {
                    _mainWindowViewModel.NavigateToMapEditor(map);

                    // Add to recent maps
                    AddToRecentMaps(map, openFileDialog.FileName);
                }
                else
                {
                    // TODO: Show error message
                    System.Windows.MessageBox.Show("Failed to load map file.", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteOpenRecentMap(object? parameter)
        {
            if (parameter is RecentMap recentMap && !string.IsNullOrEmpty(recentMap.FilePath))
            {
                var mapService = new MapService();
                var map = mapService.LoadMap(recentMap.FilePath);

                if (map != null)
                {
                    _mainWindowViewModel.NavigateToMapEditor(map);
                }
                else
                {
                    // Remove from recent maps if file no longer exists
                    RecentMaps.Remove(recentMap);

                    System.Windows.MessageBox.Show("Map file no longer exists.", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }
        }

        private void AddToRecentMaps(Map map, string filePath)
        {
            // Check if already in recent maps
            var existing = RecentMaps.FirstOrDefault(rm => rm.FilePath == filePath);
            if (existing != null)
            {
                RecentMaps.Remove(existing);
            }

            // Add to beginning of list
            RecentMaps.Insert(0, new RecentMap
            {
                Name = map.Name,
                FilePath = filePath,
                Size = $"{map.Width}x{map.Height}",
                LastModified = "Just now"
            });

            // Keep only the 5 most recent maps
            while (RecentMaps.Count > 5)
            {
                RecentMaps.RemoveAt(RecentMaps.Count - 1);
            }

            // TODO: Save recent maps to user settings
        }

        private void ExecuteSettings(object? parameter)
        {
            _mainWindowViewModel.NavigateToSettings();
        }

        private void ExecuteAbout(object? parameter)
        {
            _mainWindowViewModel.NavigateToAbout();
        }

        private void ExecuteExit(object? parameter)
        {
            System.Windows.Application.Current.Shutdown();
        }

        public void Cleanup()
        {
            _timer?.Stop();
        }
    }
}