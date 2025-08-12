using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WWXMapEditor.Views
{
    public partial class ConfigurationView : System.Windows.Controls.UserControl
    {
        public ConfigurationView()
        {
            InitializeComponent();
        }

        // Parallax strength (pixels max) — applies ONLY to the background
        private const double BgParallaxMax = 16.0;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // No-op; background animations are driven by XAML triggers.
        }

        private void UserControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var p = e.GetPosition(this);
            if (ActualWidth <= 0 || ActualHeight <= 0) return;

            // Normalize mouse positions to -1..+1 around center
            double nx = ((p.X / ActualWidth) - 0.5) * 2.0;
            double ny = ((p.Y / ActualHeight) - 0.5) * 2.0;

            // Apply gentle parallax ONLY to the background
            if (FindName("BgParallax") is TranslateTransform bg)
            {
                bg.X = -nx * BgParallaxMax;
                bg.Y = -ny * BgParallaxMax;
            }
        }

        // Clicking a tab attempts to navigate to that step
        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            var step = btn?.Tag;

            if (step == null) return;

            var dc = DataContext;
            if (dc == null) return;

            // Preferred path: ICommand NavigateToStepCommand on ViewModel
            var navigateCmdProp = dc.GetType().GetProperty("NavigateToStepCommand",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (navigateCmdProp?.GetValue(dc) is System.Windows.Input.ICommand cmd && cmd.CanExecute(step))
            {
                cmd.Execute(step);
                return;
            }

            // Fallbacks to support common patterns without breaking MVVM
            // 1) Method: NavigateToStep(object step) or GoToStep(object step)
            var navMethod = dc.GetType().GetMethod("NavigateToStep", new[] { typeof(object) }) ??
                            dc.GetType().GetMethod("GoToStep", new[] { typeof(object) });
            if (navMethod != null)
            {
                navMethod.Invoke(dc, new[] { step });
                return;
            }

            // 2) Method: NavigateToStepIndex(int) or GoToStepIndex(int) using index in ConfigurationSteps
            var stepsProp = dc.GetType().GetProperty("ConfigurationSteps");
            var steps = (stepsProp?.GetValue(dc) as System.Collections.IEnumerable)?.Cast<object>().ToList();
            if (steps != null)
            {
                int idx = steps.IndexOf(step);
                if (idx >= 0)
                {
                    var navIdx = dc.GetType().GetMethod("NavigateToStepIndex", new[] { typeof(int) }) ??
                                 dc.GetType().GetMethod("GoToStepIndex", new[] { typeof(int) });
                    if (navIdx != null)
                    {
                        navIdx.Invoke(dc, new object[] { idx });
                        return;
                    }

                    // 3) Property: SelectedStepIndex (int) or CurrentStepIndex (int)
                    var selectedIdxProp = dc.GetType().GetProperty("SelectedStepIndex") ??
                                          dc.GetType().GetProperty("CurrentStepIndex");
                    if (selectedIdxProp != null && selectedIdxProp.CanWrite)
                    {
                        selectedIdxProp.SetValue(dc, idx);
                        return;
                    }
                }
            }

            // If none of the above exist, we avoid throwing; the Previous/Next buttons still work.
        }
    }
}