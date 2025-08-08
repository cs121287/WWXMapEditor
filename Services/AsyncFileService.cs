using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WWXMapEditor.Models;

namespace WWXMapEditor.Services
{
    public interface IAsyncFileService
    {
        Task<(bool Success, Map? Map, string? Error)> LoadMapAsync(string filePath, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> SaveMapAsync(Map map, string filePath, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> ExportMapAsync(Map map, string filePath, string format, IProgress<int>? progress = null, CancellationToken cancellationToken = default);
        Task<bool> FileExistsAsync(string filePath);
        Task<(bool Success, string? Error)> CreateBackupAsync(string filePath, CancellationToken cancellationToken = default);
    }

    public class AsyncFileService : IAsyncFileService
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private const int BufferSize = 4096;
        private const long MaxFileSize = 100 * 1024 * 1024; // 100MB

        public AsyncFileService()
        {
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<(bool Success, Map? Map, string? Error)> LoadMapAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return (false, null, "File path cannot be empty");

                if (!File.Exists(filePath))
                    return (false, null, $"File not found: {filePath}");

                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > MaxFileSize)
                    return (false, null, $"File is too large (max {MaxFileSize / 1024 / 1024}MB)");

                // Read file asynchronously
                byte[] buffer;
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true))
                {
                    buffer = new byte[fileStream.Length];
                    await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                }

                // Deserialize in a background thread
                var map = await Task.Run(() =>
                {
                    using var memoryStream = new MemoryStream(buffer);
                    return JsonSerializer.Deserialize<Map>(memoryStream, _serializerOptions);
                }, cancellationToken);

                if (map == null)
                    return (false, null, "Failed to deserialize map data");

                // Validate map
                if (map.Width <= 0 || map.Height <= 0)
                    return (false, null, "Invalid map dimensions");

                if (map.Tiles == null || map.Tiles.Length == 0)
                    return (false, null, "Map contains no tiles");

                // Initialize HQs list if null
                map.HQs ??= new System.Collections.Generic.List<HQ>();

                return (true, map, null);
            }
            catch (OperationCanceledException)
            {
                return (false, null, "Operation was cancelled");
            }
            catch (UnauthorizedAccessException ex)
            {
                return (false, null, $"Access denied: {ex.Message}");
            }
            catch (IOException ex)
            {
                return (false, null, $"IO error: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return (false, null, $"Invalid map file format: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Unexpected error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? Error)> SaveMapAsync(Map map, string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (map == null)
                    return (false, "Cannot save null map");

                if (string.IsNullOrWhiteSpace(filePath))
                    return (false, "File path cannot be empty");

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create backup if file exists
                string? backupPath = null;
                if (File.Exists(filePath))
                {
                    backupPath = $"{filePath}.backup";
                    await CreateBackupAsync(filePath, cancellationToken);
                }

                // Serialize in a background thread
                var json = await Task.Run(() => 
                    JsonSerializer.SerializeToUtf8Bytes(map, _serializerOptions), 
                    cancellationToken);

                // Write file asynchronously
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, useAsync: true))
                {
                    await fileStream.WriteAsync(json, 0, json.Length, cancellationToken);
                    await fileStream.FlushAsync(cancellationToken);
                }

                // Remove backup after successful save
                if (backupPath != null && File.Exists(backupPath))
                {
                    try
                    {
                        File.Delete(backupPath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }

                return (true, null);
            }
            catch (OperationCanceledException)
            {
                return (false, "Save operation was cancelled");
            }
            catch (UnauthorizedAccessException ex)
            {
                return (false, $"Access denied: {ex.Message}");
            }
            catch (IOException ex)
            {
                return (false, $"IO error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? Error)> ExportMapAsync(Map map, string filePath, string format, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (map == null)
                    return (false, "Cannot export null map");

                if (string.IsNullOrWhiteSpace(filePath))
                    return (false, "Export path cannot be empty");

                progress?.Report(0);

                switch (format?.ToLower())
                {
                    case "json":
                        progress?.Report(50);
                        var result = await SaveMapAsync(map, filePath, cancellationToken);
                        progress?.Report(100);
                        return result;

                    case "xml":
                        return await ExportAsXmlAsync(map, filePath, progress, cancellationToken);

                    case "png":
                        return await ExportAsPngAsync(map, filePath, progress, cancellationToken);

                    default:
                        return (false, $"Unknown export format: {format}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Export failed: {ex.Message}");
            }
        }

        private async Task<(bool Success, string? Error)> ExportAsXmlAsync(Map map, string filePath, IProgress<int>? progress, CancellationToken cancellationToken)
        {
            // TODO: Implement XML export
            await Task.Delay(100, cancellationToken); // Simulate work
            progress?.Report(100);
            return (false, "XML export is not yet implemented");
        }

        private async Task<(bool Success, string? Error)> ExportAsPngAsync(Map map, string filePath, IProgress<int>? progress, CancellationToken cancellationToken)
        {
            // TODO: Implement PNG export
            await Task.Delay(100, cancellationToken); // Simulate work
            progress?.Report(100);
            return (false, "PNG export is not yet implemented");
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            return await Task.Run(() => File.Exists(filePath));
        }

        public async Task<(bool Success, string? Error)> CreateBackupAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(filePath))
                    return (false, "Source file does not exist");

                var backupPath = $"{filePath}.backup";
                
                using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true))
                using (var destStream = new FileStream(backupPath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, useAsync: true))
                {
                    await sourceStream.CopyToAsync(destStream, BufferSize, cancellationToken);
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Backup failed: {ex.Message}");
            }
        }
    }
}