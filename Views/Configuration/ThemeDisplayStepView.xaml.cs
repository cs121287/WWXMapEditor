using System.Windows;
using System.Windows.Controls;

namespace WWXMapEditor.Views.Configuration
{
    public partial class ThemeDisplayStepView : System.Windows.Controls.UserControl
    {
        public ThemeDisplayStepView()
        {
            InitializeComponent();
        }

        // Make the configuration page fullscreen by maximizing the host window when this step loads.
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var hostWindow = Window.GetWindow(this);
            if (hostWindow != null)
            {
                // Maximize the window. We intentionally keep the standard border to preserve window controls.
                hostWindow.WindowState = WindowState.Maximized;
            }
        }
    }
}