using System.Windows;

namespace WWXMapEditor.Views
{
    /// <summary>
    /// Interaction logic for NewMapView.xaml
    /// </summary>
    public partial class NewMapView : System.Windows.Controls.UserControl
    {
        public NewMapView()
        {
            InitializeComponent();
            SetupValidationMessageHandling();
        }

        private void SetupValidationMessageHandling()
        {
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ViewModels.NewMapViewModel viewModel)
            {
                if (e.OldValue is ViewModels.NewMapViewModel oldViewModel)
                {
                    oldViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                }
                viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModels.NewMapViewModel.ValidationMessage) &&
                sender is ViewModels.NewMapViewModel vm)
            {
                UpdateValidationMessageVisibility(vm.ValidationMessage);
            }
        }

        private void UpdateValidationMessageVisibility(string validationMessage)
        {
            if (FindName("ValidationMessageBorder") is System.Windows.FrameworkElement validationBorder)
            {
                validationBorder.Visibility = string.IsNullOrWhiteSpace(validationMessage)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        private void CloseValidationMessage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.NewMapViewModel vm)
            {
                vm.ValidationMessage = string.Empty;
            }
            UpdateValidationMessageVisibility(string.Empty);
        }
    }
}