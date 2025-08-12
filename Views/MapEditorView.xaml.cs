using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WWXMapEditor.Views
{
    public partial class MapEditorView : System.Windows.Controls.UserControl
    {
        public MapEditorView()
        {
            InitializeComponent();
        }

        private void OnMapMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Handled = true;
                if (DataContext is ViewModels.MapEditorViewModel viewModel)
                {
                    if (e.Delta > 0)
                        viewModel.ZoomInCommand.Execute(null);
                    else
                        viewModel.ZoomOutCommand.Execute(null);
                }
            }
        }

        private void OnMapMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DataContext is ViewModels.MapEditorViewModel viewModel)
            {
                var position = e.GetPosition(MapCanvas);
                viewModel.UpdateMousePosition(position.X, position.Y);
            }
        }

        private void OnMapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ViewModels.MapEditorViewModel viewModel)
            {
                var position = e.GetPosition(MapCanvas);
                viewModel.StartDrawing(position.X, position.Y);
            }
        }

        private void OnMapMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ViewModels.MapEditorViewModel viewModel)
            {
                viewModel.StopDrawing();
            }
        }

        private void OnMapMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ViewModels.MapEditorViewModel viewModel)
            {
                var position = e.GetPosition(MapCanvas);
                viewModel.ShowContextMenu(position.X, position.Y);
            }
        }

        private void OnMapMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DataContext is ViewModels.MapEditorViewModel viewModel)
            {
                viewModel.HideHoverIndicator();
            }
        }

        private void OnMiniMapClick(object sender, MouseButtonEventArgs e)
        {
            // Placeholder for potential minimap interaction
        }
    }
}