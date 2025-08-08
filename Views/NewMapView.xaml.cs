using System.Windows.Controls;

namespace WWXMapEditor.Views
{
    public partial class NewMapView : System.Windows.Controls.UserControl
    {
        public NewMapView()
        {
            InitializeComponent();

            // Set up validation message visibility binding
            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is ViewModels.NewMapViewModel viewModel)
                {
                    viewModel.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(viewModel.ValidationMessage))
                        {
                            if (string.IsNullOrWhiteSpace(viewModel.ValidationMessage))
                            {
                                ValidationMessageBorder.Visibility = System.Windows.Visibility.Collapsed;
                            }
                            else
                            {
                                ValidationMessageBorder.Visibility = System.Windows.Visibility.Visible;
                            }
                        }
                    };
                }
            };
        }
    }
}