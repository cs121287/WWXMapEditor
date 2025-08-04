using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WWXMapEditor.Views.Settings;
using WWXMapEditor.ViewModels.Settings;

namespace WWXMapEditor.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private int _selectedCategoryIndex;
        private object _currentSettingsPage;

        // Page ViewModels
        private readonly GeneralSettingsViewModel _generalSettingsViewModel;
        private readonly EditorSettingsViewModel _editorSettingsViewModel;
        private readonly InputSettingsViewModel _inputSettingsViewModel;
        private readonly FileProjectSettingsViewModel _fileProjectSettingsViewModel;
        private readonly DisplaySettingsViewModel _displaySettingsViewModel;
        private readonly AdvancedSettingsViewModel _advancedSettingsViewModel;

        public ObservableCollection<string> CategoryNames { get; }

        public int SelectedCategoryIndex
        {
            get => _selectedCategoryIndex;
            set
            {
                if (SetProperty(ref _selectedCategoryIndex, value))
                {
                    UpdateSettingsPage();
                }
            }
        }

        public object CurrentSettingsPage
        {
            get => _currentSettingsPage;
            set => SetProperty(ref _currentSettingsPage, value);
        }

        public ICommand CloseCommand { get; }
        public ICommand ApplyCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }

        public SettingsViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;

            // Initialize category names
            CategoryNames = new ObservableCollection<string>
            {
                "General",
                "Editor",
                "Input",
                "File & Project",
                "Display",
                "Advanced"
            };

            // Initialize page view models
            _generalSettingsViewModel = new GeneralSettingsViewModel();
            _editorSettingsViewModel = new EditorSettingsViewModel();
            _inputSettingsViewModel = new InputSettingsViewModel();
            _fileProjectSettingsViewModel = new FileProjectSettingsViewModel();
            _displaySettingsViewModel = new DisplaySettingsViewModel();
            _advancedSettingsViewModel = new AdvancedSettingsViewModel();

            CloseCommand = new RelayCommand(ExecuteClose);
            ApplyCommand = new RelayCommand(ExecuteApply);
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
            ResetToDefaultsCommand = new RelayCommand(ExecuteResetToDefaults);

            // Load all settings
            LoadAllSettings();

            // Initialize with first category
            SelectedCategoryIndex = 0;
        }

        private void UpdateSettingsPage()
        {
            switch (SelectedCategoryIndex)
            {
                case 0: // General
                    CurrentSettingsPage = new GeneralSettingsPage { DataContext = _generalSettingsViewModel };
                    break;
                case 1: // Editor
                    CurrentSettingsPage = new EditorSettingsPage { DataContext = _editorSettingsViewModel };
                    break;
                case 2: // Input
                    CurrentSettingsPage = new InputSettingsPage { DataContext = _inputSettingsViewModel };
                    break;
                case 3: // File & Project
                    CurrentSettingsPage = new FileProjectSettingsPage { DataContext = _fileProjectSettingsViewModel };
                    break;
                case 4: // Display
                    CurrentSettingsPage = new DisplaySettingsPage { DataContext = _displaySettingsViewModel };
                    break;
                case 5: // Advanced
                    CurrentSettingsPage = new AdvancedSettingsPage { DataContext = _advancedSettingsViewModel };
                    break;
            }
        }

        private void LoadAllSettings()
        {
            _generalSettingsViewModel.LoadSettings();
            _editorSettingsViewModel.LoadSettings();
            _inputSettingsViewModel.LoadSettings();
            _fileProjectSettingsViewModel.LoadSettings();
            _displaySettingsViewModel.LoadSettings();
            _advancedSettingsViewModel.LoadSettings();
        }

        private void ExecuteClose(object parameter)
        {
            _mainWindowViewModel.NavigateToMainMenu();
        }

        private void ExecuteApply(object parameter)
        {
            ApplySettings();
        }

        private void ExecuteSave(object parameter)
        {
            ApplySettings();
            SaveSettings();
            _mainWindowViewModel.NavigateToMainMenu();
        }

        private void ExecuteCancel(object parameter)
        {
            _mainWindowViewModel.NavigateToMainMenu();
        }

        private void ExecuteResetToDefaults(object parameter)
        {
            ResetSettings();
        }

        private void ApplySettings()
        {
            // Apply settings logic here
            _generalSettingsViewModel.SaveSettings();
            _editorSettingsViewModel.SaveSettings();
            _inputSettingsViewModel.SaveSettings();
            _fileProjectSettingsViewModel.SaveSettings();
            _displaySettingsViewModel.SaveSettings();
            _advancedSettingsViewModel.SaveSettings();
        }

        private void SaveSettings()
        {
            // Save settings to file
            // TODO: Implement persistent storage
        }

        private void ResetSettings()
        {
            // Reset all settings to defaults
            _generalSettingsViewModel.ResetToDefaults();
            _editorSettingsViewModel.ResetToDefaults();
            _inputSettingsViewModel.ResetToDefaults();
            _fileProjectSettingsViewModel.ResetToDefaults();
            _displaySettingsViewModel.ResetToDefaults();
            _advancedSettingsViewModel.ResetToDefaults();

            // Update the current page to reflect changes
            UpdateSettingsPage();
        }
    }
}