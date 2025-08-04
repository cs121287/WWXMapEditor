using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WWXMapEditor.Models;
using WWXMapEditor.Services;
using WWXMapEditor.Views.Configuration;

namespace WWXMapEditor.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly AppSettings _settings;
        private int _currentStepIndex = 0;
        private object _currentStepContent = null!;
        private string _progressText = "Step 1 of 4";
        private string _nextButtonText = "NEXT";
        private Visibility _previousButtonVisibility = Visibility.Collapsed;

        // Step ViewModels
        private readonly ThemeDisplayStepViewModel _themeDisplayViewModel;
        private readonly EditorPreferencesStepViewModel _editorPreferencesViewModel;
        private readonly FileLocationsStepViewModel _fileLocationsViewModel;
        private readonly ReviewFinishStepViewModel _reviewFinishViewModel;

        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            set
            {
                if (SetProperty(ref _currentStepIndex, value))
                {
                    UpdateCurrentStep();
                }
            }
        }

        public object CurrentStepContent
        {
            get => _currentStepContent;
            set => SetProperty(ref _currentStepContent, value);
        }

        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        public string NextButtonText
        {
            get => _nextButtonText;
            set => SetProperty(ref _nextButtonText, value);
        }

        public Visibility PreviousButtonVisibility
        {
            get => _previousButtonVisibility;
            set => SetProperty(ref _previousButtonVisibility, value);
        }

        public ObservableCollection<ConfigurationStep> ConfigurationSteps { get; }

        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }

        public ConfigurationViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _settings = new AppSettings();

            // Initialize configuration steps
            ConfigurationSteps = new ObservableCollection<ConfigurationStep>
            {
                new ConfigurationStep { Number = "1", Title = "Theme & Display" },
                new ConfigurationStep { Number = "2", Title = "Editor Preferences" },
                new ConfigurationStep { Number = "3", Title = "File Locations" },
                new ConfigurationStep { Number = "4", Title = "Review & Finish" }
            };

            // Initialize step view models
            _themeDisplayViewModel = new ThemeDisplayStepViewModel(_settings);
            _editorPreferencesViewModel = new EditorPreferencesStepViewModel(_settings);
            _fileLocationsViewModel = new FileLocationsStepViewModel(_settings);

            // Pass the other step view models to the review view model
            _reviewFinishViewModel = new ReviewFinishStepViewModel(
                _settings,
                _themeDisplayViewModel,
                _editorPreferencesViewModel,
                _fileLocationsViewModel);

            NextCommand = new RelayCommand(ExecuteNext);
            PreviousCommand = new RelayCommand(ExecutePrevious);

            // Initialize current step content
            _currentStepContent = new ThemeDisplayStepView { DataContext = _themeDisplayViewModel };

            UpdateCurrentStep();
        }

        private void UpdateCurrentStep()
        {
            ProgressText = $"Step {_currentStepIndex + 1} of 4";
            PreviousButtonVisibility = _currentStepIndex > 0 ? Visibility.Visible : Visibility.Collapsed;
            NextButtonText = _currentStepIndex == 3 ? "FINISH" : "NEXT";

            switch (_currentStepIndex)
            {
                case 0:
                    CurrentStepContent = new ThemeDisplayStepView { DataContext = _themeDisplayViewModel };
                    break;
                case 1:
                    CurrentStepContent = new EditorPreferencesStepView { DataContext = _editorPreferencesViewModel };
                    break;
                case 2:
                    CurrentStepContent = new FileLocationsStepView { DataContext = _fileLocationsViewModel };
                    break;
                case 3:
                    // Update the summary with current values
                    _reviewFinishViewModel.UpdateSummary();

                    // Force property change notifications to ensure UI updates
                    _reviewFinishViewModel.RefreshAllProperties();

                    CurrentStepContent = new ReviewFinishStepView { DataContext = _reviewFinishViewModel };
                    break;
            }
        }

        private void ExecuteNext(object? parameter)
        {
            if (_currentStepIndex < 3)
            {
                CurrentStepIndex++;
            }
            else
            {
                // Save configuration and navigate to main menu
                SaveConfiguration();
            }
        }

        private void ExecutePrevious(object? parameter)
        {
            if (_currentStepIndex > 0)
            {
                CurrentStepIndex--;
            }
        }

        private async void SaveConfiguration()
        {
            // Update settings from view models
            _themeDisplayViewModel.UpdateSettings();
            _editorPreferencesViewModel.UpdateSettings();
            _fileLocationsViewModel.UpdateSettings();

            // Save settings to file
            var success = await SettingsService.Instance.SaveSettingsAsync(_settings);

            if (success)
            {
                // Apply theme and settings
                SettingsService.Instance.ApplyAllSettings();

                // Navigate to main menu
                _mainWindowViewModel.NavigateToMainMenu();
            }
            else
            {
                System.Windows.MessageBox.Show("Failed to save configuration. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class ConfigurationStep
    {
        public string Number { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}