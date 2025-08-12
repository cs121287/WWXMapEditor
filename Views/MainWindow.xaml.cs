using System;
using System.Windows;
using WWXMapEditor.ViewModels;
using WWXMapEditor.Services;
using WWXMapEditor.UI.Scaling; // Adaptive scaling attachment

namespace WWXMapEditor.Views
{
    public partial class MainWindow : Window
    {
        private readonly SettingsService _settingsService;

        public MainWindow()
        {
            InitializeComponent();

            _settingsService = SettingsService.Instance;
            DataContext = new MainWindowViewModel();

            // Attach adaptive scaling (DPI / size change monitoring)
            Loaded += (_, __) => ScaleService.Instance.Attach(this);

            _settingsService.ScalingChanged += OnScalingChanged;
        }

        private void OnScalingChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(UpdateLayout);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _settingsService.ScalingChanged -= OnScalingChanged;
        }
    }
}