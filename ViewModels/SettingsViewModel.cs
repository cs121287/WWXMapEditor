using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WWXMapEditor.Services;
using WWXMapEditor.ViewModels.Settings;
using WWXMapEditor.Views.Settings;

namespace WWXMapEditor.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly MainWindowViewModel _mainViewModel;
        private readonly SettingsService _settingsService;
        private object? _currentSettingsPage;
        private int _selectedCategoryIndex;
        private ObservableCollection<string> _categoryNames;

        public SettingsViewModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _settingsService = SettingsService.Instance;

            // Initialize commands
            CloseCommand = new RelayCommand(Close);
            ApplyCommand = new RelayCommand(Apply);
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);

            // Initialize categories
            _categoryNames = new ObservableCollection<string>
            {
                "General",
                "Display",
                "Editor",
                "File & Project",
                "Input",
                "Advanced"
            };

            // Set initial category
            SelectedCategoryIndex = 0;
        }

        #region Properties
        public ObservableCollection<string> CategoryNames
        {
            get => _categoryNames;
            set
            {
                _categoryNames = value;
                OnPropertyChanged();
            }
        }

        public int SelectedCategoryIndex
        {
            get => _selectedCategoryIndex;
            set
            {
                _selectedCategoryIndex = value;
                OnPropertyChanged();
                LoadSettingsPage(value);
            }
        }

        public object? CurrentSettingsPage
        {
            get => _currentSettingsPage;
            set
            {
                _currentSettingsPage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand CloseCommand { get; }
        public ICommand ApplyCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }
        #endregion

        #region Methods
        private void LoadSettingsPage(int categoryIndex)
        {
            switch (categoryIndex)
            {
                case 0: // General
                    var generalPage = new GeneralSettingsPage();
                    generalPage.DataContext = new GeneralSettingsViewModel();
                    CurrentSettingsPage = generalPage;
                    break;

                case 1: // Display
                    var displayPage = new DisplaySettingsPage();
                    displayPage.DataContext = new DisplaySettingsViewModel();
                    CurrentSettingsPage = displayPage;
                    break;

                case 2: // Editor
                    var editorPage = new EditorSettingsPage();
                    editorPage.DataContext = new EditorSettingsViewModel();
                    CurrentSettingsPage = editorPage;
                    break;

                case 3: // File & Project
                    var filePage = new FileProjectSettingsPage();
                    filePage.DataContext = new FileProjectSettingsViewModel();
                    CurrentSettingsPage = filePage;
                    break;

                case 4: // Input
                    var inputPage = new InputSettingsPage();
                    inputPage.DataContext = new InputSettingsViewModel();
                    CurrentSettingsPage = inputPage;
                    break;

                case 5: // Advanced
                    var advancedPage = new AdvancedSettingsPage();
                    advancedPage.DataContext = new AdvancedSettingsViewModel();
                    CurrentSettingsPage = advancedPage;
                    break;

                default:
                    CurrentSettingsPage = null;
                    break;
            }
        }

        private void Close(object? parameter)
        {
            // Return to main menu without saving
            _mainViewModel.NavigateToMainMenu();
        }

        private void Apply(object? parameter)
        {
            // Apply settings without closing
            _settingsService.ApplyAllSettings();
            _mainViewModel.OnSettingsChanged();
        }

        private void Save(object? parameter)
        {
            // Save and apply settings, then close
            _settingsService.SaveSettings(_settingsService.Settings);
            _settingsService.ApplyAllSettings();
            _mainViewModel.OnSettingsChanged();
            _mainViewModel.NavigateToMainMenu();
        }

        private void Cancel(object? parameter)
        {
            // Reload settings from file and return to main menu
            _settingsService.LoadSettings();
            _settingsService.ApplyAllSettings();
            _mainViewModel.NavigateToMainMenu();
        }

        private void ResetToDefaults(object? parameter)
        {
            // Reset all settings to defaults
            _settingsService.ResetToDefaults();
            _mainViewModel.OnSettingsChanged();

            // Reload the current settings page to reflect changes
            LoadSettingsPage(_selectedCategoryIndex);
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}