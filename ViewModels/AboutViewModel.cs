using System.Windows.Input;

namespace WWXMapEditor.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        public ICommand CloseCommand { get; }

        public AboutViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        private void ExecuteClose(object parameter)
        {
            _mainWindowViewModel.NavigateToMainMenu();
        }
    }
}