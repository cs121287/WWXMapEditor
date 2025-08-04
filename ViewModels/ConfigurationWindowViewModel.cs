using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WWXMapEditor.Models;
using WWXMapEditor.Services;
using WWXMapEditor.Views;
using WWXMapEditor.Views.Configuration;

namespace WWXMapEditor.ViewModels
{
    public class ConfigurationWindowViewModel : ViewModelBase
    {
        private readonly AppSettings _settings;
        private int _currentStepIndex = 0;
        private object _currentStepContent;
        private string _progressText = "Step 1 of 4";
        private string _nextButtonText = "NEXT";
        private Visibility _previousButtonVisibility = Visibility.Collapsed;

        // Step ViewModels
        private readonly ThemeDisplayStepViewModel _themeDisplayViewModel;
        private readonly EditorPreferencesStepViewModel _editorPreferencesViewModel;
        private readonly FileLocationsStepViewModel _fileLocationsViewModel;
        private readonly ReviewFinishStepViewModel _reviewFinishViewModel;

        public event EventHandler ConfigurationCompleted;

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

        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }

        public ConfigurationWindowViewModel()
        {
            _settings = new AppSettings();

            // Initialize step view models
            _themeDisplayViewModel = new ThemeDisplayStepViewModel(_settings);
            _editorPreferencesViewModel = new EditorPreferencesStepViewModel(_settings);
            _fileLocationsViewModel = new FileLocationsStepViewModel(_settings);

            // Pass all required view models to ReviewFinishStepViewModel
            _reviewFinishViewModel = new ReviewFinishStepViewModel(
                _settings,
                _themeDisplayViewModel,
                _editorPreferencesViewModel,
                _fileLocationsViewModel);

            NextCommand = new RelayCommand(ExecuteNext);
            PreviousCommand = new RelayCommand(ExecutePrevious);

            // Set initial content to be the ConfigurationView
            CurrentStepContent = new ConfigurationView { DataContext = new ConfigurationViewModel(null) };
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
                    _reviewFinishViewModel.UpdateSummary();
                    _reviewFinishViewModel.RefreshAllProperties();
                    CurrentStepContent = new ReviewFinishStepView { DataContext = _reviewFinishViewModel };
                    break;
            }
        }

        private void ExecuteNext(object parameter)
        {
            if (_currentStepIndex < 3)
            {
                CurrentStepIndex++;
            }
            else
            {
                // Save configuration and complete
                SaveConfiguration();
            }
        }

        private void ExecutePrevious(object parameter)
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
                // Apply theme and settings immediately
                SettingsService.Instance.ApplyAllSettings();

                // Fire configuration completed event
                ConfigurationCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                System.Windows.MessageBox.Show("Failed to save configuration. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}