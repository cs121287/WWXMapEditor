using System.Windows.Input;
using WWXMapEditor.Models;
using WWXMapEditor.Views;
using WWXMapEditor.Services;

namespace WWXMapEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object _currentView = null!;

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public MainWindowViewModel()
        {
            // Don't navigate here - let App.xaml.cs handle initial navigation
        }

        public void NavigateToMainMenu()
        {
            var mainMenuView = new MainMenuView();
            var mainMenuViewModel = new MainMenuViewModel(this);
            mainMenuView.DataContext = mainMenuViewModel;
            CurrentView = mainMenuView;
        }

        public void NavigateToNewMapCreation()
        {
            var newMapView = new NewMapView();
            var newMapViewModel = new NewMapViewModel(this);
            newMapView.DataContext = newMapViewModel;
            CurrentView = newMapView;
        }

        public void NavigateToMapEditor(Map? map = null)
        {
            var mapEditorView = new MapEditorView();
            var mapEditorViewModel = new MapEditorViewModel(this, map);
            mapEditorView.DataContext = mapEditorViewModel;
            CurrentView = mapEditorView;
        }

        public void NavigateToSettings()
        {
            var settingsView = new SettingsView();
            var settingsViewModel = new SettingsViewModel(this);
            settingsView.DataContext = settingsViewModel;
            CurrentView = settingsView;
        }

        public void NavigateToAbout()
        {
            var aboutView = new AboutView();
            var aboutViewModel = new AboutViewModel(this);
            aboutView.DataContext = aboutViewModel;
            CurrentView = aboutView;
        }

        public void NavigateToConfiguration()
        {
            var configView = new ConfigurationView();
            var configViewModel = new ConfigurationViewModel(this);
            configView.DataContext = configViewModel;
            CurrentView = configView;
        }
    }
}