using System.Collections.ObjectModel;
using System.Windows.Input;

namespace WWXMapEditor.ViewModels.Settings
{
    public class InputSettingsViewModel : SettingsPageViewModelBase
    {
        private double _mouseSensitivity = 1.0;
        private bool _invertMouseWheelZoom;
        private string _middleMouseAction = "Pan";
        private string _rightClickAction = "Context menu";

        public double MouseSensitivity
        {
            get => _mouseSensitivity;
            set => SetProperty(ref _mouseSensitivity, value);
        }

        public bool InvertMouseWheelZoom
        {
            get => _invertMouseWheelZoom;
            set => SetProperty(ref _invertMouseWheelZoom, value);
        }

        public string MiddleMouseAction
        {
            get => _middleMouseAction;
            set => SetProperty(ref _middleMouseAction, value);
        }

        public string RightClickAction
        {
            get => _rightClickAction;
            set => SetProperty(ref _rightClickAction, value);
        }

        public ObservableCollection<string> MiddleMouseActions { get; }
        public ObservableCollection<string> RightClickActions { get; }
        public ObservableCollection<KeyboardShortcut> KeyboardShortcuts { get; }

        public ICommand EditShortcutCommand { get; }
        public ICommand ResetShortcutsCommand { get; }
        public ICommand ImportShortcutsCommand { get; }
        public ICommand ExportShortcutsCommand { get; }

        public InputSettingsViewModel()
        {
            MiddleMouseActions = new ObservableCollection<string> { "Pan", "Rotate", "None" };
            RightClickActions = new ObservableCollection<string> { "Context menu", "Erase", "Custom tool" };
            
            KeyboardShortcuts = new ObservableCollection<KeyboardShortcut>
            {
                new KeyboardShortcut { Action = "New Map", Shortcut = "Ctrl+N" },
                new KeyboardShortcut { Action = "Open Map", Shortcut = "Ctrl+O" },
                new KeyboardShortcut { Action = "Save Map", Shortcut = "Ctrl+S" },
                new KeyboardShortcut { Action = "Save As", Shortcut = "Ctrl+Shift+S" },
                new KeyboardShortcut { Action = "Undo", Shortcut = "Ctrl+Z" },
                new KeyboardShortcut { Action = "Redo", Shortcut = "Ctrl+Y" },
                new KeyboardShortcut { Action = "Copy", Shortcut = "Ctrl+C" },
                new KeyboardShortcut { Action = "Paste", Shortcut = "Ctrl+V" },
                new KeyboardShortcut { Action = "Cut", Shortcut = "Ctrl+X" },
                new KeyboardShortcut { Action = "Delete", Shortcut = "Del" },
                new KeyboardShortcut { Action = "Select All", Shortcut = "Ctrl+A" },
                new KeyboardShortcut { Action = "Zoom In", Shortcut = "Ctrl+Plus" },
                new KeyboardShortcut { Action = "Zoom Out", Shortcut = "Ctrl+Minus" },
                new KeyboardShortcut { Action = "Reset Zoom", Shortcut = "Ctrl+0" },
                new KeyboardShortcut { Action = "Toggle Grid", Shortcut = "G" },
                new KeyboardShortcut { Action = "Toggle Snap", Shortcut = "S" }
            };

            EditShortcutCommand = new RelayCommand(ExecuteEditShortcut);
            ResetShortcutsCommand = new RelayCommand(ExecuteResetShortcuts);
            ImportShortcutsCommand = new RelayCommand(ExecuteImportShortcuts);
            ExportShortcutsCommand = new RelayCommand(ExecuteExportShortcuts);
        }

        private void ExecuteEditShortcut(object parameter)
        {
            // TODO: Show shortcut edit dialog
        }

        private void ExecuteResetShortcuts(object parameter)
        {
            // TODO: Reset shortcuts to defaults
        }

        private void ExecuteImportShortcuts(object parameter)
        {
            // TODO: Import shortcuts from file
        }

        private void ExecuteExportShortcuts(object parameter)
        {
            // TODO: Export shortcuts to file
        }

        public override void LoadSettings()
        {
            // TODO: Load settings from configuration
        }

        public override void SaveSettings()
        {
            // TODO: Save settings to configuration
        }

        public override void ResetToDefaults()
        {
            MouseSensitivity = 1.0;
            InvertMouseWheelZoom = false;
            MiddleMouseAction = "Pan";
            RightClickAction = "Context menu";
            // Reset keyboard shortcuts
        }
    }

    public class KeyboardShortcut : ViewModelBase
    {
        private string _action;
        private string _shortcut;

        public string Action
        {
            get => _action;
            set => SetProperty(ref _action, value);
        }

        public string Shortcut
        {
            get => _shortcut;
            set => SetProperty(ref _shortcut, value);
        }
    }
}