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
            // Start with main menu
            NavigateToMainMenu();
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
            var dialog = new NewMapDialog();
            var viewModel = new NewMapDialogViewModel();
            dialog.DataContext = viewModel;

            // Subscribe to the MapCreated event
            viewModel.MapCreated += (sender, e) =>
            {
                if (viewModel.MapProperties != null)
                {
                    dialog.DialogResult = true;
                }
            };

            // Show dialog as modal
            if (dialog.ShowDialog() == true && viewModel.MapProperties != null)
            {
                // Create the map with the specified properties
                var mapService = new MapService();
                var map = mapService.CreateNewMap(viewModel.MapProperties);

                // Navigate to the map editor with the new map
                NavigateToMapEditor(map);
            }
            // If canceled, we're still on the main menu (no navigation needed)
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