using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using WWXMapEditor.Views.NewMapSteps.PlayerSelect.Support;

namespace WWXMapEditor.Views.NewMapSteps.PlayerSelect
{
    public partial class PlayerSelectWizardView : UserControl
    {
        private UniqueCountryCoordinator? _coordinator;

        public PlayerSelectWizardView()
        {
            InitializeComponent();
            Loaded += PlayerSelectWizardView_Loaded;
        }

        private void PlayerSelectWizardView_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize step and button state
            if (StepIndex < 1) StepIndex = 1;
            UpdateNextButtonText();
            UpdateBackButtonEnabled();

            // Wire uniqueness coordinator to the current DataContext (ParentViewModel or the VM itself)
            HookCoordinator();
            // React if DataContext changes later
            DataContextChanged += (_, __) => HookCoordinator();
        }

        private void HookCoordinator()
        {
            _coordinator?.Dispose();
            var vm = DataContext;
            if (vm == null) return;

            // The app often uses a wrapper exposing "ParentViewModel"
            var parent = GetPropertyValue(vm, "ParentViewModel") ?? vm;

            // Ensure properties exist; if not, coordinator will no-op
            _coordinator = new UniqueCountryCoordinator(parent,
                player1PropName: "Player1Country",
                player2PropName: "Player2Country",
                player3PropName: "Player3Country",
                player4PropName: "Player4Country",
                randomKeyword: "Random");
        }

        // StepIndex DP (1..6)
        public static readonly DependencyProperty StepIndexProperty =
            DependencyProperty.Register(
                nameof(StepIndex),
                typeof(int),
                typeof(PlayerSelectWizardView),
                new PropertyMetadata(1, OnStepIndexChanged));

        public int StepIndex
        {
            get => (int)GetValue(StepIndexProperty);
            set => SetValue(StepIndexProperty, value);
        }

        private static void OnStepIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (PlayerSelectWizardView)d;
            view.UpdateNextButtonText();
            view.UpdateBackButtonEnabled();
        }

        // Next button text DP
        public static readonly DependencyProperty NextButtonTextProperty =
            DependencyProperty.Register(
                nameof(NextButtonText),
                typeof(string),
                typeof(PlayerSelectWizardView),
                new PropertyMetadata("Next →"));

        public string NextButtonText
        {
            get => (string)GetValue(NextButtonTextProperty);
            set => SetValue(NextButtonTextProperty, value);
        }

        private void UpdateNextButtonText()
        {
            NextButtonText = StepIndex >= 6 ? "Continue →" : "Next →";
        }

        private void UpdateBackButtonEnabled()
        {
            if (BackButton != null)
                BackButton.IsEnabled = StepIndex > 1;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var players = GetNumberOfPlayers();

            switch (StepIndex)
            {
                case 1: break;
                case 2: StepIndex = 1; break;
                case 3: StepIndex = 2; break;
                case 4: StepIndex = 3; break;
                case 5: StepIndex = 4; break;
                case 6:
                    if (players >= 4) StepIndex = 5;
                    else if (players == 3) StepIndex = 4;
                    else StepIndex = 3;
                    break;
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var players = GetNumberOfPlayers();

            switch (StepIndex)
            {
                case 1: StepIndex = 2; break;                          // Number of players
                case 2: StepIndex = 3; break;                          // Player 1
                case 3: StepIndex = (players == 2) ? 6 : 4; break;     // Player 2 (+ally) -> confirm if 2p
                case 4: StepIndex = (players == 3) ? 6 : 5; break;     // Player 3 (+ally) -> confirm if 3p
                case 5: StepIndex = 6; break;                          // Player 4 (+ally)
                case 6:
                    // Hand off to parent VM (a RoutedEvent and a few common names as fallbacks)
                    if (!TryExecuteParentCommand("ContinueToVictoryConditionsCommand") &&
                        !TryInvokeParentMethod("ContinueToVictoryConditions") &&
                        !TryInvokeParentMethod("GoToVictoryConditions") &&
                        !TryInvokeParentMethod("NextFromPlayerSelect"))
                    {
                        RaiseEvent(new RoutedEventArgs(ContinueRequestedEvent));
                    }
                    break;
            }
        }

        // Routed event as a fallback signal to parent
        public static readonly RoutedEvent ContinueRequestedEvent =
            EventManager.RegisterRoutedEvent("ContinueRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PlayerSelectWizardView));

        public event RoutedEventHandler ContinueRequested
        {
            add { AddHandler(ContinueRequestedEvent, value); }
            remove { RemoveHandler(ContinueRequestedEvent, value); }
        }

        private int GetNumberOfPlayers()
        {
            var vm = DataContext;
            if (vm == null) return 2;

            var parent = GetPropertyValue(vm, "ParentViewModel") ?? vm;

            var val = GetPropertyValue(parent, "NumberOfPlayers");
            if (val is int i) return i;
            if (val is string s && int.TryParse(s, out var parsed)) return parsed;

            return 2;
        }

        private bool TryExecuteParentCommand(string commandPropertyName)
        {
            var vm = DataContext;
            if (vm == null) return false;

            var parent = GetPropertyValue(vm, "ParentViewModel") ?? vm;
            var cmdObj = GetPropertyValue(parent, commandPropertyName);
            if (cmdObj is System.Windows.Input.ICommand cmd && cmd.CanExecute(null))
            {
                cmd.Execute(null);
                return true;
            }
            return false;
        }

        private bool TryInvokeParentMethod(string methodName)
        {
            var vm = DataContext;
            if (vm == null) return false;

            var parent = GetPropertyValue(vm, "ParentViewModel") ?? vm;
            var mi = parent.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi != null)
            {
                try { mi.Invoke(parent, null); return true; }
                catch { }
            }
            return false;
        }

        private static object? GetPropertyValue(object obj, string name)
        {
            var pi = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return pi?.GetValue(obj);
        }
    }
}