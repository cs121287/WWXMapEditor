using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WWXMapEditor.Models
{
    public class AppSettings
    {
        // General Settings
        public string Theme { get; set; } = "Dark";
        public string CustomThemeColor { get; set; } = "#FF00008B"; // DarkBlue
        public string Language { get; set; } = "English";
        public bool AutoSaveEnabled { get; set; } = true;
        public int AutoSaveInterval { get; set; } = 5;
        public string AutoSaveLocation { get; set; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WWXMapEditor", "AutoSave");
        public int RecentFilesCount { get; set; } = 10;
        public List<string> RecentFiles { get; set; } = new List<string>();

        // Editor Settings
        public bool ShowGrid { get; set; } = true;
        public string GridColor { get; set; } = "#FF808080"; // Gray
        public int GridOpacity { get; set; } = 50;
        public bool SnapToGrid { get; set; } = true;
        public int GridSize { get; set; } = 32;
        public int DefaultMapWidth { get; set; } = 50;
        public int DefaultMapHeight { get; set; } = 50;
        public string CanvasBackgroundColor { get; set; } = "#FF333333";
        public bool ShowTileCoordinates { get; set; } = false;
        public bool ShowRulers { get; set; } = true;
        public bool HardwareAcceleration { get; set; } = true;
        public string UndoHistoryLimit { get; set; } = "100";
        public string TextureQuality { get; set; } = "High";
        public bool SmoothZooming { get; set; } = true;

        // Input Settings
        public double MouseSensitivity { get; set; } = 1.0;
        public bool InvertMouseWheelZoom { get; set; } = false;
        public string MiddleMouseAction { get; set; } = "Pan";
        public string RightClickAction { get; set; } = "Context menu";
        public Dictionary<string, string> KeyboardShortcuts { get; set; } = new Dictionary<string, string>
        {
            { "New Map", "Ctrl+N" },
            { "Open Map", "Ctrl+O" },
            { "Save Map", "Ctrl+S" },
            { "Save As", "Ctrl+Shift+S" },
            { "Undo", "Ctrl+Z" },
            { "Redo", "Ctrl+Y" },
            { "Copy", "Ctrl+C" },
            { "Paste", "Ctrl+V" },
            { "Cut", "Ctrl+X" },
            { "Delete", "Del" },
            { "Select All", "Ctrl+A" },
            { "Zoom In", "Ctrl+Plus" },
            { "Zoom Out", "Ctrl+Minus" },
            { "Reset Zoom", "Ctrl+0" },
            { "Toggle Grid", "G" },
            { "Toggle Snap", "S" }
        };

        // File & Project Settings
        public string DefaultProjectDirectory { get; set; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WWXMapEditor", "Projects");
        public string DefaultTilesetDirectory { get; set; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WWXMapEditor", "Tilesets");
        public string DefaultExportDirectory { get; set; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WWXMapEditor", "Exports");
        public string TemplatesDirectory { get; set; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WWXMapEditor", "Templates");
        public string DefaultSaveFormat { get; set; } = ".wwx";
        public string CompressionLevel { get; set; } = "Fast";
        public bool IncludeMetadata { get; set; } = true;
        public bool BackupOnSave { get; set; } = true;

        // Display Settings
        public bool StartInFullscreen { get; set; } = true;
        public bool RememberWindowPosition { get; set; } = true;
        public string MultiMonitorBehavior { get; set; } = "Primary monitor";
        public string UIScaling { get; set; } = "100%";
        public string FontSize { get; set; } = "Medium";
        public bool ShowTooltips { get; set; } = true;
        public int TooltipDelay { get; set; } = 500;
        public int WindowX { get; set; } = 0;
        public int WindowY { get; set; } = 0;
        public int WindowWidth { get; set; } = 1920;
        public int WindowHeight { get; set; } = 1080;

        // Advanced Settings
        public bool ShowFPSCounter { get; set; } = false;
        public bool ShowMemoryUsage { get; set; } = false;
        public bool EnableDebugConsole { get; set; } = false;
        public string LogLevel { get; set; } = "Warning";
        public bool EnablePlugins { get; set; } = false;
        public string PluginDirectory { get; set; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WWXMapEditor", "Plugins");

        // Configuration Metadata
        [JsonIgnore]
        public bool IsFirstRun { get; set; } = true;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "1.0.0";
    }
}