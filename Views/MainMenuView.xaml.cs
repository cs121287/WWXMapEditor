using System.Linq;

namespace WWXMapEditor.Views
{
    // Fully qualified WPF types to avoid ambiguity with WinForms.
    public partial class MainMenuView : System.Windows.Controls.UserControl
    {
        public MainMenuView()
        {
            InitializeComponent();
            _gamepad = new GamepadNavigator(this);
        }

        // DependencyProperty for the side label text ("Main Menu" or current hovered button text)
        public static readonly System.Windows.DependencyProperty HoverLabelTextProperty =
            System.Windows.DependencyProperty.Register(
                nameof(HoverLabelText),
                typeof(string),
                typeof(MainMenuView),
                new System.Windows.PropertyMetadata("MAIN MENU", OnHoverLabelTextChanged));

        public string HoverLabelText
        {
            get => (string)GetValue(HoverLabelTextProperty);
            set => SetValue(HoverLabelTextProperty, value);
        }

        private static void OnHoverLabelTextChanged(System.Windows.DependencyObject d, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var view = (MainMenuView)d;
            view.AnimateSideLabelChangeSafe();
        }

        // Parallax strength (pixels max) — applies ONLY to the background
        private const double BgParallaxMax = 18.0;

        // Optional gamepad scaffold (safe no-op by default)
        private readonly GamepadNavigator _gamepad;

        // Track hovered button to decide label; we ignore keyboard focus for label per requirements
        private System.Windows.Controls.Button _hoveredButton;

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Default label text
            HoverLabelText = "MAIN MENU";

            // Focus the first actionable button (no wedge will show unless hovered)
            var first = this.FindName("BtnNewMap") as System.Windows.Controls.Button;
            first?.Focus();

            _gamepad?.Start();
        }

        private void UserControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var p = e.GetPosition(this);
            if (this.ActualWidth <= 0 || this.ActualHeight <= 0) return;

            // Normalize mouse position to -1..+1 range around center
            double nx = ((p.X / this.ActualWidth) - 0.5) * 2.0;
            double ny = ((p.Y / this.ActualHeight) - 0.5) * 2.0;

            // Apply gentle parallax ONLY to the background
            if (this.FindName("BgParallax") is System.Windows.Media.TranslateTransform bg)
            {
                bg.X = -nx * BgParallaxMax;
                bg.Y = -ny * BgParallaxMax;
            }
        }

        private void UserControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Up/Down arrows move focus across the slat buttons, Enter activates.
            // Per request, the side label only follows mouse hover, not keyboard focus.
            var menuStackObj = this.FindName("MenuStack") as System.Windows.Controls.Panel;
            if (menuStackObj == null) return;

            var buttons = menuStackObj.Children
                                      .OfType<System.Windows.Controls.Button>()
                                      .Where(b => b.IsEnabled && b.IsVisible)
                                      .ToList();
            if (buttons.Count == 0) return;

            int idx = buttons.FindIndex(b => b.IsKeyboardFocused);
            if (idx < 0)
            {
                if (e.Key == System.Windows.Input.Key.Down || e.Key == System.Windows.Input.Key.Up)
                {
                    buttons[0].Focus();
                    e.Handled = true;
                }
                return;
            }

            if (e.Key == System.Windows.Input.Key.Down)
            {
                int next = (idx + 1) % buttons.Count;
                buttons[next].Focus();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                int prev = (idx - 1 + buttons.Count) % buttons.Count;
                buttons[prev].Focus();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Space)
            {
                var focused = buttons[idx];
                if (focused.Command != null)
                {
                    var parameter = focused.CommandParameter;
                    if (focused.Command.CanExecute(parameter))
                        focused.Command.Execute(parameter);
                }
                else
                {
                    focused.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                }
                e.Handled = true;
            }
        }

        // Button event handlers to drive the side label (hover-only behavior)
        private void MenuButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _hoveredButton = sender as System.Windows.Controls.Button;
            if (_hoveredButton != null)
            {
                HoverLabelText = GetButtonLabel(_hoveredButton);
            }
        }

        private void MenuButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_hoveredButton == sender)
                _hoveredButton = null;

            // If no button is hovered, show "Main Menu"
            if (_hoveredButton == null)
                HoverLabelText = "Main Menu";
        }

        private static string GetButtonLabel(System.Windows.Controls.Button btn)
        {
            if (btn == null) return "Main Menu";
            var text = btn.Content as string ?? btn.Content?.ToString() ?? "Main Menu";
            return string.IsNullOrWhiteSpace(text) ? "Main Menu" : text;
        }

        // Safe wrapper to avoid throwing if storyboard isn't loaded yet
        private void AnimateSideLabelChangeSafe()
        {
            // If view isn't loaded yet, delay until after Loaded to ensure resources are available
            if (!this.IsLoaded)
            {
                this.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Loaded,
                    new System.Action(AnimateSideLabelChangeSafe));
                return;
            }

            // Prepare initial state
            var tb = this.FindName("SideLabelText") as System.Windows.Controls.TextBlock;
            var tx = this.FindName("SideLabelTx") as System.Windows.Media.TranslateTransform;
            var sc = this.FindName("SideLabelScale") as System.Windows.Media.ScaleTransform;

            if (tb == null || tx == null || sc == null)
                return;

            tb.Opacity = 0;
            tx.Y = 6;
            sc.ScaleX = 0.96;
            sc.ScaleY = 0.96;

            // Look up storyboard without throwing; fall back to direct animations if missing
            var res = this.TryFindResource("SideLabelChangeStoryboard") as System.Windows.Media.Animation.Storyboard;
            if (res != null)
            {
                var sb = res.Clone();
                sb.Begin(this, true);
            }
            else
            {
                // Fallback: equivalent animations in code
                var dur = System.TimeSpan.FromMilliseconds(180);

                var daOpacity = new System.Windows.Media.Animation.DoubleAnimation(0, 1, dur);
                tb.BeginAnimation(System.Windows.UIElement.OpacityProperty, daOpacity);

                var daScaleX = new System.Windows.Media.Animation.DoubleAnimation(0.96, 1, dur);
                var daScaleY = new System.Windows.Media.Animation.DoubleAnimation(0.96, 1, dur);
                sc.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, daScaleX);
                sc.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, daScaleY);

                var daY = new System.Windows.Media.Animation.DoubleAnimation(6, 0, dur);
                tx.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, daY);
            }
        }

        // Optional, lightweight gamepad navigation scaffold.
        // This compiles without external dependencies and does nothing unless you plug in a provider.
        private sealed class GamepadNavigator
        {
            private readonly System.Windows.Controls.UserControl _owner;
            private System.Windows.Threading.DispatcherTimer _timer;

            public GamepadNavigator(System.Windows.Controls.UserControl owner)
            {
                _owner = owner;
            }

            public void Start()
            {
                // No-op timer by default. Uncomment and implement Poll() if you wire a provider.
                _timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = System.TimeSpan.FromMilliseconds(80)
                };
                _timer.Tick += (s, e) => Poll();
                // _timer.Start(); // disabled by default; enable after wiring a provider
            }

            // Plug your gamepad provider here (e.g., SharpDX.XInput via reflection).
            private void Poll()
            {
                // Example scaffold (pseudo):
                // var state = _provider.TryGetState();
                // if (state.NavigationDownPressed) MoveFocus(+1);
                // if (state.NavigationUpPressed) MoveFocus(-1);
                // if (state.AcceptPressed) ActivateFocused();
            }

            private void MoveFocus(int delta)
            {
                var stack = _owner.FindName("MenuStack") as System.Windows.Controls.Panel;
                if (stack == null) return;

                var buttons = stack.Children.OfType<System.Windows.Controls.Button>()
                                            .Where(b => b.IsEnabled && b.IsVisible)
                                            .ToList();
                if (buttons.Count == 0) return;

                int idx = buttons.FindIndex(b => b.IsKeyboardFocused);
                if (idx < 0) idx = 0;

                int next = (idx + delta + buttons.Count) % buttons.Count;
                buttons[next].Focus();
            }

            private void ActivateFocused()
            {
                var focused = System.Windows.Input.Keyboard.FocusedElement as System.Windows.Controls.Button;
                if (focused == null) return;

                if (focused.Command != null)
                {
                    var parameter = focused.CommandParameter;
                    if (focused.Command.CanExecute(parameter))
                        focused.Command.Execute(parameter);
                }
                else
                {
                    focused.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                }
            }
        }
    }
}