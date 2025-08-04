using System.Windows;
using System.Windows.Controls;

namespace WWXMapEditor.Views
{
    public partial class NewMapDialog : Window
    {
        public NewMapDialog()
        {
            InitializeComponent();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}