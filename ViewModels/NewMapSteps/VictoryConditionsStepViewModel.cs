namespace WWXMapEditor.ViewModels
{
    public class VictoryConditionsStepViewModel : ViewModelBase
    {
        private readonly NewMapViewModel _parentViewModel;

        public  NewMapViewModel ParentViewModel => _parentViewModel;

        public VictoryConditionsStepViewModel(NewMapViewModel parentViewModel)
        {
            _parentViewModel = parentViewModel;
        }
    }
}