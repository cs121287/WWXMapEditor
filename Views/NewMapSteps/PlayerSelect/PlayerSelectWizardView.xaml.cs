using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WWXMapEditor.ViewModels;

namespace WWXMapEditor.Views.NewMapSteps.PlayerSelect
{
    public partial class PlayerSelectWizardView : System.Windows.Controls.UserControl
    {
        public event EventHandler? ContinueRequested;

        private PlayerSelectStepViewModel? VM => DataContext as PlayerSelectStepViewModel;

        public PlayerSelectWizardView()
        {
            InitializeComponent();

            Loaded += (_, __) =>
            {
                HookVM();
                RefreshButtons();
            };

            DataContextChanged += (_, __) =>
            {
                HookVM();
                RefreshButtons();
            };
        }

        private void HookVM()
        {
            if (VM != null)
            {
                VM.PropertyChanged -= VMOnPropertyChanged;
                VM.PropertyChanged += VMOnPropertyChanged;

                // Ensure a sane starting step
                if (VM.StepIndex < 1 || VM.StepIndex > 6)
                    VM.StepIndex = 1;
            }
        }

        private void VMOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayerSelectStepViewModel.StepIndex))
            {
                // Triggers on StepHost will handle template switching automatically.
                RefreshButtons();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (VM == null) return;
            var parent = VM.ParentViewModel;
            int current = VM.StepIndex;
            int players = parent.NumberOfPlayers;

            VM.StepIndex = ComputePrevStep(current, players);
            RefreshButtons();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (VM == null) return;
            var parent = VM.ParentViewModel;
            int current = VM.StepIndex;
            int players = parent.NumberOfPlayers;

            int next = ComputeNextStep(current, players);
            if (current == 6 && next == 6)
            {
                // At confirmation step; bubble up to advance outer wizard
                ContinueRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            VM.StepIndex = next;
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            if (VM == null) return;

            int current = VM.StepIndex;
            BtnBack.IsEnabled = current > 1;
            BtnNext.Content = current == 6 ? "Continue →" : "Next →";
        }

        // Navigation rules for inner steps:
        // 1: PlayerCount
        // 2: Player1
        // 3: Player2
        // 4: Player3 (only if players >= 3)
        // 5: Player4 (only if players == 4)
        // 6: Confirm
        private static int ComputeNextStep(int current, int players)
        {
            switch (current)
            {
                case 1: return 2;
                case 2: return 3;
                case 3: return players >= 3 ? 4 : 6;
                case 4: return players >= 4 ? 5 : 6;
                case 5: return 6;
                case 6: return 6;
                default: return 1;
            }
        }

        private static int ComputePrevStep(int current, int players)
        {
            switch (current)
            {
                case 1: return 1;
                case 2: return 1;
                case 3: return 2;
                case 4: return 3;
                case 5: return 4;
                case 6:
                    if (players == 4) return 5;
                    if (players == 3) return 4;
                    return 3;
                default: return 1;
            }
        }
    }
}