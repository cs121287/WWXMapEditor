using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WWXMapEditor.Models;
using WWXMapEditor.Views.NewMapSteps;
using WWXMapEditor.Services;

namespace WWXMapEditor.ViewModels
{
    public class NewMapViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private int _currentStepIndex = 0;
        private object _currentStepContent = null!;
        private string _progressText = "Step 1 of 3";
        private string _nextButtonText = "NEXT";
        private Visibility _previousButtonVisibility = Visibility.Collapsed;
        private string _validationMessage = "";

        // Step ViewModels
        private readonly BasicInformationStepViewModel _basicInfoViewModel;
        private readonly VictoryConditionsStepViewModel _victoryConditionsViewModel;
        private readonly FogOfWarStepViewModel _fogOfWarViewModel;

        // Map properties
        private string _mapName = "Untitled Map";
        private string _mapDescription = "";
        private int _mapWidth = 50;
        private int _mapHeight = 50;
        private string _selectedTerrain = "Plains";
        private int _numberOfPlayers = 2;
        private bool _eliminationVictory = true;
        private bool _captureObjectivesVictory = false;
        private bool _survivalVictory = false;
        private bool _economicVictory = false;
        private bool _fogOfWarEnabled = true;
        private string _shroudType = "Black";
        private double _visionPenaltyMultiplier = 1.0;
        private string _visionPenaltyMultiplierString = "Default (1x)";

        public string MapName
        {
            get => _mapName;
            set => SetProperty(ref _mapName, value);
        }

        public string MapDescription
        {
            get => _mapDescription;
            set => SetProperty(ref _mapDescription, value);
        }

        public int MapWidth
        {
            get => _mapWidth;
            set
            {
                if (value >= 10 && value <= 500)
                {
                    SetProperty(ref _mapWidth, value);
                    ValidateInput();
                }
            }
        }

        public int MapHeight
        {
            get => _mapHeight;
            set
            {
                if (value >= 10 && value <= 500)
                {
                    SetProperty(ref _mapHeight, value);
                    ValidateInput();
                }
            }
        }

        public string SelectedTerrain
        {
            get => _selectedTerrain;
            set => SetProperty(ref _selectedTerrain, value);
        }

        public int NumberOfPlayers
        {
            get => _numberOfPlayers;
            set => SetProperty(ref _numberOfPlayers, value);
        }

        public bool EliminationVictory
        {
            get => _eliminationVictory;
            set
            {
                SetProperty(ref _eliminationVictory, value);
                ValidateVictoryConditions();
            }
        }

        public bool CaptureObjectivesVictory
        {
            get => _captureObjectivesVictory;
            set
            {
                SetProperty(ref _captureObjectivesVictory, value);
                ValidateVictoryConditions();
            }
        }

        public bool SurvivalVictory
        {
            get => _survivalVictory;
            set
            {
                SetProperty(ref _survivalVictory, value);
                ValidateVictoryConditions();
            }
        }

        public bool EconomicVictory
        {
            get => _economicVictory;
            set
            {
                SetProperty(ref _economicVictory, value);
                ValidateVictoryConditions();
            }
        }

        public bool FogOfWarEnabled
        {
            get => _fogOfWarEnabled;
            set => SetProperty(ref _fogOfWarEnabled, value);
        }

        public string ShroudType
        {
            get => _shroudType;
            set => SetProperty(ref _shroudType, value);
        }

        public double VisionPenaltyMultiplier
        {
            get => _visionPenaltyMultiplier;
            set => SetProperty(ref _visionPenaltyMultiplier, value);
        }

        public string VisionPenaltyMultiplierString
        {
            get => _visionPenaltyMultiplierString;
            set
            {
                if (SetProperty(ref _visionPenaltyMultiplierString, value))
                {
                    // Update the numeric value based on selection
                    switch (value)
                    {
                        case "Default (1x)":
                            VisionPenaltyMultiplier = 1.0;
                            break;
                        case "2x":
                            VisionPenaltyMultiplier = 2.0;
                            break;
                        case "3x":
                            VisionPenaltyMultiplier = 3.0;
                            break;
                    }
                }
            }
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set => SetProperty(ref _validationMessage, value);
        }

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

        public ObservableCollection<string> TerrainTypes { get; }
        public ObservableCollection<int> PlayerCountOptions { get; }
        public ObservableCollection<string> ShroudTypes { get; }
        public ObservableCollection<string> VisionPenaltyMultipliers { get; }
        public ObservableCollection<MapConfigurationStep> ConfigurationSteps { get; }

        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ClearValidationMessageCommand { get; }

        public NewMapViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;

            TerrainTypes = new ObservableCollection<string> { "Plains", "Mountain", "Forest", "Sand", "Sea" };
            PlayerCountOptions = new ObservableCollection<int> { 2, 3, 4, 5, 6 };
            ShroudTypes = new ObservableCollection<string> { "Black", "Grey" };
            VisionPenaltyMultipliers = new ObservableCollection<string> { "Default (1x)", "2x", "3x" };

            // Initialize configuration steps
            ConfigurationSteps = new ObservableCollection<MapConfigurationStep>
            {
                new MapConfigurationStep
                {
                    StepNumber = 1,
                    Number = "1",
                    Title = "Basic Information",
                    Description = "Set the fundamental properties of your map",
                    IsActive = true,
                    IsCompleted = false,
                    IsLast = false
                },
                new MapConfigurationStep
                {
                    StepNumber = 2,
                    Number = "2",
                    Title = "Victory Conditions",
                    Description = "Define how players can achieve victory",
                    IsActive = false,
                    IsCompleted = false,
                    IsLast = false
                },
                new MapConfigurationStep
                {
                    StepNumber = 3,
                    Number = "3",
                    Title = "Fog of War",
                    Description = "Configure visibility and exploration settings",
                    IsActive = false,
                    IsCompleted = false,
                    IsLast = true
                }
            };

            // Initialize step view models
            _basicInfoViewModel = new BasicInformationStepViewModel(this);
            _victoryConditionsViewModel = new VictoryConditionsStepViewModel(this);
            _fogOfWarViewModel = new FogOfWarStepViewModel(this);

            // Initialize commands
            NextCommand = new RelayCommand(ExecuteNext, CanExecuteNext);
            PreviousCommand = new RelayCommand(ExecutePrevious, CanExecutePrevious);
            CancelCommand = new RelayCommand(ExecuteCancel);
            ClearValidationMessageCommand = new RelayCommand(ExecuteClearValidationMessage);

            UpdateCurrentStep();
        }

        private void UpdateCurrentStep()
        {
            // Update step states
            for (int i = 0; i < ConfigurationSteps.Count; i++)
            {
                ConfigurationSteps[i].IsActive = i == CurrentStepIndex;
                ConfigurationSteps[i].IsCompleted = i < CurrentStepIndex;
            }

            ProgressText = $"Step {CurrentStepIndex + 1} of {ConfigurationSteps.Count}";
            PreviousButtonVisibility = CurrentStepIndex > 0 ? Visibility.Visible : Visibility.Collapsed;
            NextButtonText = CurrentStepIndex == ConfigurationSteps.Count - 1 ? "CREATE MAP" : "NEXT →";

            // Clear validation message when changing steps
            ValidationMessage = "";

            switch (CurrentStepIndex)
            {
                case 0:
                    CurrentStepContent = new BasicInformationStepView { DataContext = _basicInfoViewModel };
                    break;
                case 1:
                    CurrentStepContent = new VictoryConditionsStepView { DataContext = _victoryConditionsViewModel };
                    break;
                case 2:
                    CurrentStepContent = new FogOfWarStepView { DataContext = _fogOfWarViewModel };
                    break;
            }
        }

        private void ValidateInput()
        {
            ValidationMessage = "";

            if (MapWidth < 10 || MapWidth > 500)
            {
                ValidationMessage = "Map width must be between 10 and 500 tiles";
            }
            else if (MapHeight < 10 || MapHeight > 500)
            {
                ValidationMessage = "Map height must be between 10 and 500 tiles";
            }
        }

        private void ValidateVictoryConditions()
        {
            if (!EliminationVictory && !CaptureObjectivesVictory && !SurvivalVictory && !EconomicVictory)
            {
                ValidationMessage = "At least one victory condition must be selected";
            }
            else
            {
                ValidationMessage = "";
            }
        }

        private bool CanExecuteNext(object? parameter)
        {
            switch (CurrentStepIndex)
            {
                case 0: // Basic Information
                    return !string.IsNullOrWhiteSpace(MapName) &&
                           MapWidth >= 10 && MapWidth <= 500 &&
                           MapHeight >= 10 && MapHeight <= 500;
                case 1: // Victory Conditions
                    return EliminationVictory || CaptureObjectivesVictory || SurvivalVictory || EconomicVictory;
                case 2: // Fog of War - Last step
                    return true;
                default:
                    return false;
            }
        }

        private void ExecuteNext(object? parameter)
        {
            if (CurrentStepIndex < ConfigurationSteps.Count - 1)
            {
                CurrentStepIndex++;
            }
            else
            {
                // Last step - create the map
                CreateMap();
            }
        }

        private bool CanExecutePrevious(object? parameter)
        {
            return CurrentStepIndex > 0;
        }

        private void ExecutePrevious(object? parameter)
        {
            if (CurrentStepIndex > 0)
            {
                CurrentStepIndex--;
            }
        }

        private void ExecuteCancel(object? parameter)
        {
            // Show confirmation dialog
            var result = System.Windows.MessageBox.Show(
                "Are you sure you want to cancel? All unsaved changes will be lost.",
                "Cancel Map Creation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Navigate back to main menu
                _mainWindowViewModel.NavigateToMainMenu();
            }
        }

        private void ExecuteClearValidationMessage(object? parameter)
        {
            ValidationMessage = string.Empty;
        }

        private void CreateMap()
        {
            try
            {
                var mapProperties = new MapProperties
                {
                    Name = MapName,
                    Description = MapDescription,
                    Width = MapWidth,
                    Height = MapHeight,
                    StartingTerrain = SelectedTerrain,
                    NumberOfPlayers = NumberOfPlayers,
                    VictoryConditions = new VictoryConditions
                    {
                        Elimination = EliminationVictory,
                        CaptureObjectives = CaptureObjectivesVictory,
                        Survival = SurvivalVictory,
                        Economic = EconomicVictory
                    },
                    FogOfWarSettings = new FogOfWarSettings
                    {
                        Enabled = FogOfWarEnabled,
                        ShroudType = ShroudType,
                        VisionPenaltyMultiplier = VisionPenaltyMultiplier
                    }
                };

                // Create the map with the specified properties
                var mapService = new MapService();
                var map = mapService.CreateNewMap(mapProperties);

                // Show success message
                System.Windows.MessageBox.Show(
                    $"Map '{MapName}' has been created successfully!",
                    "Map Created",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Navigate to the map editor with the new map
                _mainWindowViewModel.NavigateToMapEditor(map);
            }
            catch (Exception ex)
            {
                // Show error message
                ValidationMessage = $"Failed to create map: {ex.Message}";
                System.Windows.MessageBox.Show(
                    $"An error occurred while creating the map:\n\n{ex.Message}",
                    "Map Creation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    // Additional supporting class for map configuration steps
    public class MapConfigurationStep : ViewModelBase
    {
        private int _stepNumber;
        private string _number = "";
        private string _title = "";
        private string _description = "";
        private bool _isActive;
        private bool _isCompleted;
        private bool _isLast;

        public int StepNumber
        {
            get => _stepNumber;
            set => SetProperty(ref _stepNumber, value);
        }

        public string Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        public bool IsLast
        {
            get => _isLast;
            set => SetProperty(ref _isLast, value);
        }
    }
}