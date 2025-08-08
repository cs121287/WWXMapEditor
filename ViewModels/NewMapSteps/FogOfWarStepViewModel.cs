namespace WWXMapEditor.ViewModels
{
    public class FogOfWarStepViewModel : ViewModelBase
    {
        private readonly NewMapViewModel _parentViewModel;

        public NewMapViewModel ParentViewModel => _parentViewModel;

        public FogOfWarStepViewModel(NewMapViewModel parentViewModel)
        {
            _parentViewModel = parentViewModel;
        }
    }
}