using System;
using System.Windows.Input;
using WWXMapEditor.Models;
using WWXMapEditor.Views;
using WWXMapEditor.Services;

namespace WWXMapEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object _currentView = null!;
        private IDisposable? _currentViewModel;

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public MainWindowViewModel()
        {
            // Don't navigate here - let App.xaml.cs handle initial navigation
        }

        private void CleanupCurrentViewModel()
        {
            // Dispose of current view model if it implements IDisposable
            if (_currentViewModel != null)
            {
                _currentViewModel.Dispose();
                _currentViewModel = null;
            }
        }

        public void NavigateToMainMenu()
        {
            CleanupCurrentViewModel();
            
            var mainMenuView = new MainMenuView();
            var mainMenuViewModel = new MainMenuViewModel(this);
            mainMenuView.DataContext = mainMenuViewModel;
            
            _currentViewModel = mainMenuViewModel;
            CurrentView = mainMenuView;
        }

        public void NavigateToNewMapCreation()
        {
            CleanupCurrentViewModel();
            
            var newMapView = new NewMapView();
            var newMapViewModel = new NewMapViewModel(this);
            newMapView.DataContext = newMapViewModel;
            
            _currentViewModel = newMapViewModel as IDisposable;
            CurrentView = newMapView;
        }

        public void NavigateToMapEditor(Map? map = null)
        {
            CleanupCurrentViewModel();
            
            var mapEditorView = new MapEditorView();
            var mapEditorViewModel = new MapEditorViewModel(this, map);
            mapEditorView.DataContext = mapEditorViewModel;
            
            _currentViewModel = mapEditorViewModel;
            CurrentView = mapEditorView;
        }

        public void NavigateToSettings()
        {
            CleanupCurrentViewModel();
            
            var settingsView = new SettingsView();
            var settingsViewModel = new SettingsViewModel(this);
            settingsView.DataContext = settingsViewModel;
            
            _currentViewModel = settingsViewModel as IDisposable;
            CurrentView = settingsView;
        }

        public void NavigateToAbout()
        {
            CleanupCurrentViewModel();
            
            var aboutView = new AboutView();
            var aboutViewModel = new AboutViewModel(this);
            aboutView.DataContext = aboutViewModel;
            
            _currentViewModel = aboutViewModel as IDisposable;
            CurrentView = aboutView;
        }

        public void NavigateToConfiguration()
        {
            CleanupCurrentViewModel();
            
            var configView = new ConfigurationView();
            var configViewModel = new ConfigurationViewModel(this);
            configView.DataContext = configViewModel;
            
            _currentViewModel = configViewModel as IDisposable;
            CurrentView = configView;
        }
    }
}