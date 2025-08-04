namespace WWXMapEditor.ViewModels
{
    public class BasicInformationStepViewModel : ViewModelBase
    {
        private readonly NewMapDialogViewModel _parentViewModel;

        public NewMapDialogViewModel ParentViewModel => _parentViewModel;

        public BasicInformationStepViewModel(NewMapDialogViewModel parentViewModel)
        {
            _parentViewModel = parentViewModel;
        }
    }
}