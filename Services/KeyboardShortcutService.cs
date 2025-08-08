using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace WWXMapEditor.Services
{
    public interface IKeyboardShortcutService
    {
        void RegisterShortcut(string action, Key key, ModifierKeys modifiers = ModifierKeys.None);
        void UnregisterShortcut(string action);
        bool IsShortcut(string action, Key key, ModifierKeys modifiers);
        string GetShortcutDisplay(string action);
        Dictionary<string, ShortcutInfo> GetAllShortcuts();
        void LoadDefaults();
    }

    public class ShortcutInfo
    {
        public Key Key { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public string Display { get; set; } = string.Empty;
    }

    public class KeyboardShortcutService : IKeyboardShortcutService
    {
        private readonly Dictionary<string, ShortcutInfo> _shortcuts = new Dictionary<string, ShortcutInfo>();

        public KeyboardShortcutService()
        {
            LoadDefaults();
        }

        public void LoadDefaults()
        {
            // File operations
            RegisterShortcut("NewMap", Key.N, ModifierKeys.Control);
            RegisterShortcut("OpenMap", Key.O, ModifierKeys.Control);
            RegisterShortcut("SaveMap", Key.S, ModifierKeys.Control);
            RegisterShortcut("SaveAsMap", Key.S, ModifierKeys.Control | ModifierKeys.Shift);
            RegisterShortcut("ExportMap", Key.E, ModifierKeys.Control | ModifierKeys.Shift);
            RegisterShortcut("Exit", Key.F4, ModifierKeys.Alt);

            // Edit operations
            RegisterShortcut("Undo", Key.Z, ModifierKeys.Control);
            RegisterShortcut("Redo", Key.Y, ModifierKeys.Control);
            RegisterShortcut("Cut", Key.X, ModifierKeys.Control);
            RegisterShortcut("Copy", Key.C, ModifierKeys.Control);
            RegisterShortcut("Paste", Key.V, ModifierKeys.Control);
            RegisterShortcut("Delete", Key.Delete);
            RegisterShortcut("SelectAll", Key.A, ModifierKeys.Control);

            // View operations
            RegisterShortcut("ZoomIn", Key.Add, ModifierKeys.Control);
            RegisterShortcut("ZoomOut", Key.Subtract, ModifierKeys.Control);
            RegisterShortcut("ResetZoom", Key.D0, ModifierKeys.Control);
            RegisterShortcut("ToggleGrid", Key.G);
            RegisterShortcut("ToggleSnap", Key.S, ModifierKeys.Alt);
            RegisterShortcut("ToggleRulers", Key.R, ModifierKeys.Control);

            // Tools
            RegisterShortcut("SelectTool", Key.V);
            RegisterShortcut("BrushTool", Key.B);
            RegisterShortcut("EraserTool", Key.E);
            RegisterShortcut("FillTool", Key.F);
            RegisterShortcut("RulerTool", Key.M);
            RegisterShortcut("RectangleTool", Key.R);

            // Map operations
            RegisterShortcut("ValidateMap", Key.F5);
            RegisterShortcut("TestMap", Key.F6);
            RegisterShortcut("QuickSave", Key.S, ModifierKeys.Control | ModifierKeys.Alt);

            // Navigation
            RegisterShortcut("NavigateUp", Key.Up);
            RegisterShortcut("NavigateDown", Key.Down);
            RegisterShortcut("NavigateLeft", Key.Left);
            RegisterShortcut("NavigateRight", Key.Right);
            RegisterShortcut("NavigatePageUp", Key.PageUp);
            RegisterShortcut("NavigatePageDown", Key.PageDown);

            // Brush size
            RegisterShortcut("IncreaseBrushSize", Key.OemCloseBrackets);
            RegisterShortcut("DecreaseBrushSize", Key.OemOpenBrackets);

            // Layer switching
            RegisterShortcut("Layer1", Key.D1);
            RegisterShortcut("Layer2", Key.D2);
            RegisterShortcut("Layer3", Key.D3);
            RegisterShortcut("Layer4", Key.D4);

            // Help
            RegisterShortcut("Help", Key.F1);
            RegisterShortcut("About", Key.F1, ModifierKeys.Shift);
        }

        public void RegisterShortcut(string action, Key key, ModifierKeys modifiers = ModifierKeys.None)
        {
            _shortcuts[action] = new ShortcutInfo
            {
                Key = key,
                Modifiers = modifiers,
                Display = GetDisplayString(key, modifiers)
            };
        }

        public void UnregisterShortcut(string action)
        {
            _shortcuts.Remove(action);
        }

        public bool IsShortcut(string action, Key key, ModifierKeys modifiers)
        {
            if (!_shortcuts.TryGetValue(action, out var shortcut))
                return false;

            return shortcut.Key == key && shortcut.Modifiers == modifiers;
        }

        public string GetShortcutDisplay(string action)
        {
            return _shortcuts.TryGetValue(action, out var shortcut) ? shortcut.Display : string.Empty;
        }

        public Dictionary<string, ShortcutInfo> GetAllShortcuts()
        {
            return new Dictionary<string, ShortcutInfo>(_shortcuts);
        }

        private string GetDisplayString(Key key, ModifierKeys modifiers)
        {
            var parts = new List<string>();

            if (modifiers.HasFlag(ModifierKeys.Control))
                parts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Alt))
                parts.Add("Alt");
            if (modifiers.HasFlag(ModifierKeys.Shift))
                parts.Add("Shift");
            if (modifiers.HasFlag(ModifierKeys.Windows))
                parts.Add("Win");

            parts.Add(GetKeyDisplayString(key));

            return string.Join("+", parts);
        }

        private string GetKeyDisplayString(Key key)
        {
            return key switch
            {
                Key.Add => "+",
                Key.Subtract => "-",
                Key.OemOpenBrackets => "[",
                Key.OemCloseBrackets => "]",
                Key.OemComma => ",",
                Key.OemPeriod => ".",
                Key.OemQuestion => "?",
                Key.OemSemicolon => ";",
                Key.OemQuotes => "'",
                Key.OemPipe => "\\",
                Key.OemTilde => "`",
                Key.Delete => "Del",
                Key.PageUp => "PgUp",
                Key.PageDown => "PgDn",
                _ => key.ToString()
            };
        }
    }
}