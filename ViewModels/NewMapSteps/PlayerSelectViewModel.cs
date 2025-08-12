namespace WWXMapEditor.ViewModels
{
    public class PlayerSelectStepViewModel : ViewModelBase
    {
        private readonly NewMapViewModel _parentViewModel;

        public NewMapViewModel ParentViewModel => _parentViewModel;

        public PlayerSelectStepViewModel(NewMapViewModel parentViewModel)
        {
            _parentViewModel = parentViewModel;
        }
    }
}