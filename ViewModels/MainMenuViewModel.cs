using System;
using System.Windows.Input;
using WWXMapEditor.Services;
using WWXMapEditor.Views;

namespace WWXMapEditor.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        public ICommand NewMapCommand { get; }
        public ICommand LoadMapCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand ExitCommand { get; }

        public MainMenuViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;

            NewMapCommand = new RelayCommand(ExecuteNewMap);
            LoadMapCommand = new RelayCommand(ExecuteLoadMap);
            SettingsCommand = new RelayCommand(ExecuteSettings);
            AboutCommand = new RelayCommand(ExecuteAbout);
            ExitCommand = new RelayCommand(ExecuteExit);
        }

        private void ExecuteNewMap(object? parameter)
        {
            // Use the MainWindowViewModel's navigation method to handle the dialog
            _mainWindowViewModel.NavigateToNewMapCreation();
        }

        private void ExecuteLoadMap(object? parameter)
        {
            // TODO: Implement load map functionality
            _mainWindowViewModel.NavigateToMapEditor();
        }

        private void ExecuteSettings(object? parameter)
        {
            _mainWindowViewModel.NavigateToSettings();
        }

        private void ExecuteAbout(object? parameter)
        {
            _mainWindowViewModel.NavigateToAbout();
        }

        private void ExecuteExit(object? parameter)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}