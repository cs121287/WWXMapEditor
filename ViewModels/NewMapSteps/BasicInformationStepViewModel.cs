namespace WWXMapEditor.ViewModels
{
    public class BasicInformationStepViewModel : ViewModelBase
    {
        private readonly NewMapViewModel _parentViewModel;

        public NewMapViewModel ParentViewModel => _parentViewModel;

        public BasicInformationStepViewModel(NewMapViewModel parentViewModel)
        {
            _parentViewModel = parentViewModel;
        }
    }
}