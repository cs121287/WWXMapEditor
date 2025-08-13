using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace WWXMapEditor.Views.NewMapSteps
{
    public partial class PlayerSelectStepView : System.Windows.Controls.UserControl
    {
        public PlayerSelectStepView()
        {
            InitializeComponent();
            // Initialize to Step 1 by default
            if (StepIndex < 1) StepIndex = 1;
        }

        // StepIndex as a DependencyProperty so XAML bindings can find it at compile-time/design-time.
        public static readonly DependencyProperty StepIndexProperty =
            DependencyProperty.Register(
                nameof(StepIndex),
                typeof(int),
                typeof(PlayerSelectStepView),
                new PropertyMetadata(1, OnStepIndexChanged));

        public int StepIndex
        {
            get => (int)GetValue(StepIndexProperty);
            set => SetValue(StepIndexProperty, value);
        }

        private static void OnStepIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (PlayerSelectStepView)d;
            view.UpdateNextButtonText();
            view.UpdateBackButtonEnabled();
        }

        // NextButtonText as a DependencyProperty to satisfy XAML binding
        public static readonly DependencyProperty NextButtonTextProperty =
            DependencyProperty.Register(
                nameof(NextButtonText),
                typeof(string),
                typeof(PlayerSelectStepView),
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

        // Navigation handlers
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var players = GetNumberOfPlayers();

            switch (StepIndex)
            {
                case 1:
                    // Already first step
                    break;
                case 2:
                    StepIndex = 1;
                    break;
                case 3:
                    StepIndex = 2;
                    break;
                case 4:
                    StepIndex = 3;
                    break;
                case 5:
                    StepIndex = 4;
                    break;
                case 6:
                    if (players >= 4) StepIndex = 5;
                    else if (players == 3) StepIndex = 4;
                    else StepIndex = 3; // players == 2
                    break;
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var players = GetNumberOfPlayers();

            switch (StepIndex)
            {
                case 1:
                    StepIndex = 2;
                    break;
                case 2:
                    StepIndex = 3;
                    break;
                case 3:
                    StepIndex = (players == 2) ? 6 : 4;
                    break;
                case 4:
                    StepIndex = (players == 3) ? 6 : 5;
                    break;
                case 5:
                    StepIndex = 6;
                    break;
                case 6:
                    // Continue to victory conditions
                    if (!TryExecuteParentCommand("ContinueToVictoryConditionsCommand") &&
                        !TryInvokeParentMethod("ContinueToVictoryConditions") &&
                        !TryInvokeParentMethod("GoToVictoryConditions") &&
                        !TryInvokeParentMethod("NextFromPlayerSelect"))
                    {
                        // Fallback: raise a routed event parents can handle
                        RaiseEvent(new RoutedEventArgs(ContinueRequestedEvent));
                    }
                    break;
            }
        }

        // Routed event as a fallback signal to parent
        public static readonly RoutedEvent ContinueRequestedEvent =
            EventManager.RegisterRoutedEvent("ContinueRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PlayerSelectStepView));

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
                try
                {
                    mi.Invoke(parent, null);
                    return true;
                }
                catch
                {
                    // ignore and return false
                }
            }
            return false;
        }

        private static object GetPropertyValue(object obj, string name)
        {
            if (obj == null) return null;
            var pi = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return pi?.GetValue(obj);
        }
    }
}