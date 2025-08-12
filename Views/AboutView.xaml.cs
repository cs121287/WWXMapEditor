using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WWXMapEditor.Views
{
    public partial class AboutView : System.Windows.Controls.UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }

        // Match ConfigurationView background parallax behavior
        private const double BgParallaxMax = 16.0;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // No-op: Background animations are declared in XAML.
        }

        private void UserControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var p = e.GetPosition(this);
            if (ActualWidth <= 0 || ActualHeight <= 0) return;

            double nx = ((p.X / ActualWidth) - 0.5) * 2.0;
            double ny = ((p.Y / ActualHeight) - 0.5) * 2.0;

            if (FindName("BgParallax") is TranslateTransform bg)
            {
                bg.X = -nx * BgParallaxMax;
                bg.Y = -ny * BgParallaxMax;
            }
        }
    }
}