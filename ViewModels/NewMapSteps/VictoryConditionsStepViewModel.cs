namespace WWXMapEditor.ViewModels
{
    public class VictoryConditionsStepViewModel : ViewModelBase
    {
        private readonly NewMapDialogViewModel _parentViewModel;

        public NewMapDialogViewModel ParentViewModel => _parentViewModel;

        public VictoryConditionsStepViewModel(NewMapDialogViewModel parentViewModel)
        {
            _parentViewModel = parentViewModel;
        }
    }
}