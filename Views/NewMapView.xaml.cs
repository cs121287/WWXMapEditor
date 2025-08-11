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
            // Set up validation message visibility binding
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ViewModels.NewMapViewModel viewModel)
            {
                // Unsubscribe from previous view model if exists
                if (e.OldValue is ViewModels.NewMapViewModel oldViewModel)
                {
                    oldViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                }

                // Subscribe to new view model
                viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModels.NewMapViewModel.ValidationMessage))
            {
                if (sender is ViewModels.NewMapViewModel viewModel)
                {
                    UpdateValidationMessageVisibility(viewModel.ValidationMessage);
                }
            }
        }

        private void UpdateValidationMessageVisibility(string validationMessage)
        {
            // Access the ValidationMessageBorder element from XAML
            if (this.FindName("ValidationMessageBorder") is System.Windows.FrameworkElement validationBorder)
            {
                validationBorder.Visibility = string.IsNullOrWhiteSpace(validationMessage)
                    ? System.Windows.Visibility.Collapsed
                    : System.Windows.Visibility.Visible;
            }
        }

        private void CloseValidationMessage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.NewMapViewModel viewModel)
            {
                viewModel.ValidationMessage = string.Empty;
            }

            UpdateValidationMessageVisibility(string.Empty);
        }
    }
}