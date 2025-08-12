using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using WWXMapEditor.Services;

namespace WWXMapEditor.UI.Scaling
{
    /// <summary>
    /// Computes effective UI scaling and font scaling based on:
    /// - Window size relative to design size
    /// - Monitor DPI
    ///
    /// Singleton with Instance static property for easy XAML binding:
    ///   Source="{x:Static scaling:ScaleService.Instance}" Path="FontScale"
    ///
    /// Call Attach(window) once from your main Window to keep metrics updated.
    /// </summary>
    public sealed class ScaleService : INotifyPropertyChanged
    {
        private static readonly Lazy<ScaleService> _lazy = new(() => new ScaleService());
        public static ScaleService Instance => _lazy.Value;

        // Optional: listen to SettingsService for any scaling-related changes.
        private readonly SettingsService _settingsService;

        private double _effectiveScale = 1.0;
        private double _fontScale = 1.0;
        private LayoutDensity _layoutDensity = LayoutDensity.Normal;

        // Public knobs (safe defaults, can be overridden by your SettingsService externally if desired)
        public string ScaleMode { get; set; } = "Automatic";    // "Automatic" | "SystemDpiOnly" | "CustomFixed" | "LegacyPercent"
        public double CustomFixedScale { get; set; } = 1.0;      // used when ScaleMode == CustomFixed

        public double DesignWidth { get; set; } = 1920;
        public double DesignHeight { get; set; } = 1080;

        public double MinAutoScale { get; set; } = 0.75;
        public double MaxAutoScale { get; set; } = 2.0;

        public double MinFontScale { get; set; } = 0.85;
        public double MaxFontScale { get; set; } = 1.8;

        public bool EnableBreakpointStyling { get; set; } = true;

        // Current metrics
        public double CurrentWindowWidth { get; private set; }
        public double CurrentWindowHeight { get; private set; }
        public double CurrentMonitorDpi { get; private set; } = 96.0;

        private Window? _attachedWindow;

        // Debounce
        private DateTime _lastCompute = DateTime.MinValue;
        private readonly TimeSpan _minInterval = TimeSpan.FromMilliseconds(50);

        private ScaleService()
        {
            // Wire up to SettingsService if you want recompute triggers when user toggles settings.
            _settingsService = SettingsService.Instance;
            // MainWindow already listens to SettingsService.ScalingChanged. We also listen here to recompute scales.
            _settingsService.ScalingChanged += (_, __) => Recompute();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public double EffectiveScale
        {
            get => _effectiveScale;
            private set => Set(ref _effectiveScale, value);
        }

        public double FontScale
        {
            get => _fontScale;
            private set => Set(ref _fontScale, value);
        }

        public LayoutDensity LayoutDensity
        {
            get => _layoutDensity;
            private set => Set(ref _layoutDensity, value);
        }

        /// <summary>
        /// Attach to a Window to auto-update when size or DPI changes.
        /// Safe to call multiple times; it will detach from prior window.
        /// </summary>
        public void Attach(Window window)
        {
            if (_attachedWindow != null)
            {
                _attachedWindow.SizeChanged -= OnWindowSizeChanged;
                _attachedWindow.SourceInitialized -= OnWindowSourceInitialized;
                _attachedWindow.Loaded -= OnWindowLoaded;
            }

            _attachedWindow = window;

            // Hook events
            window.SizeChanged += OnWindowSizeChanged;
            window.SourceInitialized += OnWindowSourceInitialized;
            window.Loaded += OnWindowLoaded;

            // Initialize metrics immediately if possible
            UpdateMetricsFromWindow(window);
            Recompute();
        }

        /// <summary>
        /// Manually update metrics (useful for custom hosts or tests).
        /// </summary>
        public void UpdateWindowMetrics(double windowWidth, double windowHeight, double monitorDpi)
        {
            CurrentWindowWidth = windowWidth;
            CurrentWindowHeight = windowHeight;
            CurrentMonitorDpi = monitorDpi;
            Recompute();
        }

        /// <summary>
        /// Force a recompute using current metrics and settings.
        /// </summary>
        public void Recompute()
        {
            var now = DateTime.UtcNow;
            if (now - _lastCompute < _minInterval) return;
            _lastCompute = now;

            // Use safe, non-nullable values
            double designW = DesignWidth > 0 ? DesignWidth : 1920;
            double designH = DesignHeight > 0 ? DesignHeight : 1080;

            // Physical scaling factor based on window size vs. design size
            double widthFactor = CurrentWindowWidth > 0 ? (CurrentWindowWidth / designW) : 1.0;
            double heightFactor = CurrentWindowHeight > 0 ? (CurrentWindowHeight / designH) : 1.0;
            double physicalScale = (widthFactor + heightFactor) / 2.0;

            // DPI factor relative to 96
            double dpiScale = CurrentMonitorDpi > 0 ? (CurrentMonitorDpi / 96.0) : 1.0;

            // Determine base scale based on ScaleMode
            double autoScale;
            var mode = (ScaleMode ?? "Automatic").Trim().ToLowerInvariant();
            switch (mode)
            {
                case "systemdpionly":
                    autoScale = dpiScale;
                    break;

                case "customfixed":
                    autoScale = CustomFixedScale > 0 ? CustomFixedScale : 1.0;
                    break;

                case "legacypercent":
                    // If you support a legacy percent externally, you can set CustomFixedScale to that value before calling Recompute()
                    autoScale = CustomFixedScale > 0 ? CustomFixedScale : 1.0;
                    break;

                case "automatic":
                default:
                    autoScale = physicalScale * dpiScale;
                    break;
            }

            // Clamp within configured bounds
            double minAuto = MinAutoScale > 0 ? MinAutoScale : 0.75;
            double maxAuto = MaxAutoScale > 0 ? MaxAutoScale : 2.0;
            if (maxAuto < minAuto) maxAuto = Math.Max(minAuto, 1.65);
            autoScale = Math.Clamp(autoScale, minAuto, maxAuto);

            // Compute font scale (apply a small easing to avoid extremes)
            const double gamma = 0.95;
            double computedFontScale = Math.Pow(autoScale, gamma);

            // Clamp font scale
            double minFont = MinFontScale > 0 ? MinFontScale : 0.85;
            double maxFont = MaxFontScale > 0 ? MaxFontScale : 1.8;
            if (maxFont < minFont) maxFont = Math.Max(minFont, 1.2);
            computedFontScale = Math.Clamp(computedFontScale, minFont, maxFont);

            EffectiveScale = autoScale;
            FontScale = computedFontScale;

            // Simple density thresholds (optional)
            if (EnableBreakpointStyling)
            {
                LayoutDensity density = LayoutDensity.Normal;
                if (autoScale < 0.85) density = LayoutDensity.Compact;
                else if (autoScale > 1.15) density = LayoutDensity.Spacious;
                LayoutDensity = density;
            }

            // Diagnostics
            OnPropertyChanged(nameof(CurrentWindowWidth));
            OnPropertyChanged(nameof(CurrentWindowHeight));
            OnPropertyChanged(nameof(CurrentMonitorDpi));
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (_attachedWindow is null) return;
            UpdateMetricsFromWindow(_attachedWindow);
            Recompute();
        }

        private void OnWindowSourceInitialized(object? sender, EventArgs e)
        {
            if (_attachedWindow is null) return;
            UpdateMetricsFromWindow(_attachedWindow);
            Recompute();
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_attachedWindow is null) return;
            UpdateMetricsFromWindow(_attachedWindow);
            Recompute();
        }

        private void UpdateMetricsFromWindow(Window window)
        {
            CurrentWindowWidth = Math.Max(1, window.ActualWidth);
            CurrentWindowHeight = Math.Max(1, window.ActualHeight);
            CurrentMonitorDpi = GetDpiForVisual(window);
        }

        private static double GetDpiForVisual(Visual visual)
        {
            try
            {
                var source = PresentationSource.FromVisual(visual);
                if (source?.CompositionTarget != null)
                {
                    // M11 and M22 are DPI scale factors (96-based)
                    var m = source.CompositionTarget.TransformToDevice;
                    return 96.0 * m.M11; // Use X as DPI
                }
            }
            catch
            {
                // ignore and return default
            }
            return 96.0;
        }

        private bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}