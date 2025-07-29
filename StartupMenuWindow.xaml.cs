using System.Windows;
using Microsoft.Win32;

namespace WwXMapEditor
{
    public partial class StartupMenuWindow : Window
    {
        public StartupMenuWindow()
        {
            InitializeComponent();
        }

        private void CreateNewMap_Click(object sender, RoutedEventArgs e)
        {
            var optionsWindow = new NewMapOptionsWindow();
            optionsWindow.Owner = this;
            if (optionsWindow.ShowDialog() == true)
            {
                var mainWindow = new MainWindow(optionsWindow.MapOptions);
                mainWindow.Show();
                this.Close();
            }
        }

        private void LoadMap_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "JSON Map Files (*.json)|*.json|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                var mainWindow = new MainWindow(dlg.FileName);
                mainWindow.Show();
                this.Close();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}