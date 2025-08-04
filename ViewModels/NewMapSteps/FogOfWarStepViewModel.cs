namespace WWXMapEditor.ViewModels
{
    public class FogOfWarStepViewModel : ViewModelBase
    {
        private readonly NewMapDialogViewModel _parentViewModel;

        public NewMapDialogViewModel ParentViewModel => _parentViewModel;

        public FogOfWarStepViewModel(NewMapDialogViewModel parentViewModel)
        {
            _parentViewModel = parentViewModel;
        }
    }
}