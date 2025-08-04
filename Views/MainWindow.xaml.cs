using System.Windows;
using WWXMapEditor.ViewModels;

namespace WWXMapEditor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}