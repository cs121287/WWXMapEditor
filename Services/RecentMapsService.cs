using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WWXMapEditor.Models;

namespace WWXMapEditor.Services
{
    public interface IRecentMapsService
    {
        ObservableCollection<RecentMapInfo> RecentMaps { get; }
        Task LoadRecentMapsAsync();
        Task AddRecentMapAsync(string filePath, Map map);
        Task RemoveRecentMapAsync(string filePath);
        Task ClearRecentMapsAsync();
        Task ValidateRecentMapsAsync();
        bool IsRecentMapValid(string filePath);
    }

    public class RecentMapInfo
    {
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime LastOpened { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Dimensions { get; set; } = string.Empty;
        public int PlayerCount { get; set; }
        public bool IsValid { get; set; } = true;
        public string Thumbnail { get; set; } = "/Assets/Images/map_thumbnail_default.png";
    }

    public class RecentMapsService : IRecentMapsService
    {
        private readonly SettingsService _settingsService;
        private readonly MapService _mapService;
        private readonly string _thumbnailsPath;

        public ObservableCollection<RecentMapInfo> RecentMaps { get; }

        public RecentMapsService()
        {
            _settingsService = SettingsService.Instance;
            _mapService = new MapService();
            RecentMaps = new ObservableCollection<RecentMapInfo>();

            // Setup thumbnails directory
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WWXMapEditor");
            _thumbnailsPath = Path.Combine(appDataPath, "Thumbnails");
            
            if (!Directory.Exists(_thumbnailsPath))
            {
                try
                {
                    Directory.CreateDirectory(_thumbnailsPath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create thumbnails directory: {ex.Message}");
                }
            }
        }

        public async Task LoadRecentMapsAsync()
        {
            await Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke(() => RecentMaps.Clear());

                var settings = _settingsService.Settings;
                if (settings.RecentMaps == null || settings.RecentMaps.Count == 0)
                {
                    return;
                }

                var validMaps = new List<RecentMapInfo>();

                foreach (var mapPath in settings.RecentMaps.Take(settings.RecentFilesCount))
                {
                    if (File.Exists(mapPath))
                    {
                        try
                        {
                            var fileInfo = new FileInfo(mapPath);
                            var mapInfo = new RecentMapInfo
                            {
                                FilePath = mapPath,
                                Name = Path.GetFileNameWithoutExtension(mapPath),
                                LastOpened = fileInfo.LastAccessTime,
                                Size = FormatFileSize(fileInfo.Length),
                                IsValid = true
                            };

                            // Try to load map details
                            try
                            {
                                var result = _mapService.LoadMap(mapPath);
                                if (result.Success && result.Data != null)
                                {
                                    var map = result.Data;
                                    mapInfo.Name = map.Name;
                                    mapInfo.Dimensions = $"{map.Width}×{map.Height}";
                                    mapInfo.PlayerCount = map.NumberOfPlayers;

                                    // Check for thumbnail
                                    var thumbnailPath = GetThumbnailPath(mapPath);
                                    if (File.Exists(thumbnailPath))
                                    {
                                        mapInfo.Thumbnail = thumbnailPath;
                                    }
                                }
                            }
                            catch
                            {
                                // If we can't load the map, just use file info
                                mapInfo.Dimensions = "Unknown";
                            }

                            validMaps.Add(mapInfo);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error loading recent map {mapPath}: {ex.Message}");
                        }
                    }
                    else
                    {
                        // File doesn't exist, add as invalid
                        validMaps.Add(new RecentMapInfo
                        {
                            FilePath = mapPath,
                            Name = Path.GetFileNameWithoutExtension(mapPath) + " (File not found)",
                            IsValid = false,
                            LastOpened = DateTime.MinValue
                        });
                    }
                }

                // Sort by last opened date
                validMaps = validMaps.OrderByDescending(m => m.LastOpened).ToList();

                // Update UI on dispatcher thread
                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var map in validMaps)
                    {
                        RecentMaps.Add(map);
                    }
                });
            });
        }

        public async Task AddRecentMapAsync(string filePath, Map map)
        {
            if (string.IsNullOrEmpty(filePath) || map == null) return;

            await Task.Run(() =>
            {
                var settings = _settingsService.Settings;
                
                // Remove if already exists
                settings.RecentMaps.Remove(filePath);
                
                // Add to beginning
                settings.RecentMaps.Insert(0, filePath);
                
                // Limit to configured count
                while (settings.RecentMaps.Count > settings.RecentFilesCount)
                {
                    settings.RecentMaps.RemoveAt(settings.RecentMaps.Count - 1);
                }
                
                // Save settings
                _settingsService.SaveSettings(settings);

                // Generate thumbnail
                GenerateThumbnail(filePath, map);
            });

            // Reload recent maps
            await LoadRecentMapsAsync();
        }

        public async Task RemoveRecentMapAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            await Task.Run(() =>
            {
                var settings = _settingsService.Settings;
                settings.RecentMaps.Remove(filePath);
                _settingsService.SaveSettings(settings);

                // Remove thumbnail if exists
                var thumbnailPath = GetThumbnailPath(filePath);
                if (File.Exists(thumbnailPath))
                {
                    try
                    {
                        File.Delete(thumbnailPath);
                    }
                    catch
                    {
                        // Ignore thumbnail deletion errors
                    }
                }
            });

            // Remove from collection
            var mapToRemove = RecentMaps.FirstOrDefault(m => m.FilePath == filePath);
            if (mapToRemove != null)
            {
                RecentMaps.Remove(mapToRemove);
            }
        }

        public async Task ClearRecentMapsAsync()
        {
            await Task.Run(() =>
            {
                var settings = _settingsService.Settings;
                settings.RecentMaps.Clear();
                _settingsService.SaveSettings(settings);

                // Clear all thumbnails
                try
                {
                    if (Directory.Exists(_thumbnailsPath))
                    {
                        Directory.Delete(_thumbnailsPath, true);
                        Directory.CreateDirectory(_thumbnailsPath);
                    }
                }
                catch
                {
                    // Ignore errors
                }
            });

            RecentMaps.Clear();
        }

        public async Task ValidateRecentMapsAsync()
        {
            var invalidMaps = new List<string>();

            await Task.Run(() =>
            {
                foreach (var map in RecentMaps)
                {
                    if (!File.Exists(map.FilePath))
                    {
                        map.IsValid = false;
                        invalidMaps.Add(map.FilePath);
                    }
                }
            });

            // Remove invalid maps from settings
            if (invalidMaps.Any())
            {
                var settings = _settingsService.Settings;
                foreach (var invalidPath in invalidMaps)
                {
                    settings.RecentMaps.Remove(invalidPath);
                }
                _settingsService.SaveSettings(settings);
            }
        }

        public bool IsRecentMapValid(string filePath)
        {
            return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string GetThumbnailPath(string mapPath)
        {
            var hash = mapPath.GetHashCode().ToString("X");
            return Path.Combine(_thumbnailsPath, $"{hash}.png");
        }

        private void GenerateThumbnail(string mapPath, Map map)
        {
            // TODO: Implement actual thumbnail generation
            // For now, we'll just use the default thumbnail
        }
    }
}