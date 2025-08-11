using System;
using System.Windows;
using WWXMapEditor.ViewModels;
using WWXMapEditor.Services;

namespace WWXMapEditor.Views
{
    public partial class MainWindow : Window
    {
        private readonly SettingsService _settingsService;

        public MainWindow()
        {
            InitializeComponent();

            _settingsService = SettingsService.Instance;

            // Set up the DataContext
            DataContext = new MainWindowViewModel();

            // Subscribe to scaling changes if needed
            _settingsService.ScalingChanged += OnScalingChanged;
        }

        private void OnScalingChanged(object? sender, EventArgs e)
        {
            // Force layout update to ensure scaled resources are applied
            Dispatcher.Invoke(() =>
            {
                UpdateLayout();
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _settingsService.ScalingChanged -= OnScalingChanged;
        }
    }
}