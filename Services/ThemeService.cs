using System;
using System.Windows;
using System.Windows.Media;

namespace WWXMapEditor.Services
{
    public class ThemeService
    {
        private static ThemeService _instance;
        public static ThemeService Instance => _instance ??= new ThemeService();

        private ResourceDictionary _currentTheme;
        private readonly ResourceDictionary _darkTheme;
        private readonly ResourceDictionary _lightTheme;

        public event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        public enum Theme
        {
            Dark,
            Light,
            Custom
        }

        public Theme CurrentTheme { get; private set; } = Theme.Dark;

        private ThemeService()
        {
            // Use component pack URIs so themes resolve regardless of caller
            _darkTheme = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/WWXMapEditor;component/Resources/Themes/DarkTheme.xaml", UriKind.Absolute)
            };

            _lightTheme = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/WWXMapEditor;component/Resources/Themes/LightTheme.xaml", UriKind.Absolute)
            };

            _currentTheme = _darkTheme;
        }

        public void SetTheme(Theme theme)
        {
            ResourceDictionary newTheme = theme switch
            {
                Theme.Light => _lightTheme,
                Theme.Dark => _darkTheme,
                _ => _darkTheme
            };

            ApplyTheme(newTheme, theme);
        }

        private void ApplyTheme(ResourceDictionary newTheme, Theme theme)
        {
            var app = System.Windows.Application.Current;
            if (app == null) return;

            // Get the application resources
            var appResources = app.Resources;

            // Find and remove the current theme dictionary
            ResourceDictionary themeToRemove = null;

            for (int i = appResources.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                var dict = appResources.MergedDictionaries[i];
                if (dict?.Source != null)
                {
                    var sourceString = dict.Source.ToString();
                    if (sourceString.Contains("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase) ||
                        sourceString.Contains("LightTheme.xaml", StringComparison.OrdinalIgnoreCase))
                    {
                        themeToRemove = dict;
                        break;
                    }
                }
                else if (dict == _currentTheme)
                {
                    themeToRemove = dict;
                    break;
                }
            }

            // Remove the old theme
            if (themeToRemove != null)
            {
                appResources.MergedDictionaries.Remove(themeToRemove);
            }

            // Add the new theme at the beginning so view-level resources can override if needed
            appResources.MergedDictionaries.Insert(0, newTheme);

            _currentTheme = newTheme;
            CurrentTheme = theme;

            // Force refresh of all dynamic resources on the visual tree
            if (app.MainWindow != null)
            {
                RefreshDynamicResources(app.MainWindow);
            }

            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(theme));
        }

        private void RefreshDynamicResources(DependencyObject obj)
        {
            if (obj == null) return;

            // Safely trigger re-evaluation of DynamicResource without wiping local resources
            if (obj is FrameworkElement element)
            {
                element.InvalidateProperty(FrameworkElement.StyleProperty);
                element.InvalidateVisual();
                element.UpdateLayout();
            }

            // Recursively update children
            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                RefreshDynamicResources(child);
            }
        }

        public void SetCustomTheme(System.Windows.Media.Color primaryColor)
        {
            // Create a custom theme based on the provided color
            var customTheme = new ResourceDictionary();

            // Generate a color palette based on the primary color
            customTheme["BackgroundColor"] = GenerateBackgroundColor(primaryColor);
            customTheme["SurfaceColor"] = GenerateSurfaceColor(primaryColor);
            customTheme["Surface2Color"] = GenerateSurface2Color(primaryColor);
            customTheme["Surface3Color"] = GenerateSurface3Color(primaryColor);
            customTheme["BorderColor"] = GenerateBorderColor(primaryColor);
            customTheme["ForegroundColor"] = GenerateForegroundColor(primaryColor);
            customTheme["ForegroundSecondaryColor"] = GenerateForegroundSecondaryColor(primaryColor);
            customTheme["AccentColor"] = primaryColor;
            customTheme["HoverColor"] = GenerateHoverColor(primaryColor);
            customTheme["PressedColor"] = GeneratePressedColor(primaryColor);

            // Create brushes
            foreach (var key in customTheme.Keys)
            {
                if (key.ToString().EndsWith("Color", StringComparison.Ordinal))
                {
                    var brushKey = key.ToString().Replace("Color", "Brush", StringComparison.Ordinal);
                    customTheme[brushKey] = new SolidColorBrush((System.Windows.Media.Color)customTheme[key]);
                }
            }

            ApplyTheme(customTheme, Theme.Custom);
        }

        private System.Windows.Media.Color GenerateBackgroundColor(System.Windows.Media.Color primary)
        {
            // Logic to generate background color based on primary
            var brightness = (primary.R + primary.G + primary.B) / 3.0 / 255.0;
            return brightness > 0.5 ? Colors.White : Colors.Black;
        }

        private System.Windows.Media.Color GenerateSurfaceColor(System.Windows.Media.Color primary)
        {
            var brightness = (primary.R + primary.G + primary.B) / 3.0 / 255.0;
            return brightness > 0.5 ? System.Windows.Media.Color.FromRgb(255, 255, 255) : System.Windows.Media.Color.FromRgb(26, 26, 26);
        }

        private System.Windows.Media.Color GenerateSurface2Color(System.Windows.Media.Color primary)
        {
            var brightness = (primary.R + primary.G + primary.B) / 3.0 / 255.0;
            return brightness > 0.5 ? System.Windows.Media.Color.FromRgb(240, 240, 240) : System.Windows.Media.Color.FromRgb(42, 42, 42);
        }

        private System.Windows.Media.Color GenerateSurface3Color(System.Windows.Media.Color primary)
        {
            var brightness = (primary.R + primary.G + primary.B) / 3.0 / 255.0;
            return brightness > 0.5 ? System.Windows.Media.Color.FromRgb(232, 232, 232) : System.Windows.Media.Color.FromRgb(51, 51, 51);
        }

        private System.Windows.Media.Color GenerateBorderColor(System.Windows.Media.Color primary)
        {
            var brightness = (primary.R + primary.G + primary.B) / 3.0 / 255.0;
            return brightness > 0.5 ? System.Windows.Media.Color.FromRgb(204, 204, 204) : System.Windows.Media.Color.FromRgb(85, 85, 85);
        }

        private System.Windows.Media.Color GenerateForegroundColor(System.Windows.Media.Color primary)
        {
            var brightness = (primary.R + primary.G + primary.B) / 3.0 / 255.0;
            return brightness > 0.5 ? Colors.Black : Colors.White;
        }

        private System.Windows.Media.Color GenerateForegroundSecondaryColor(System.Windows.Media.Color primary)
        {
            var brightness = (primary.R + primary.G + primary.B) / 3.0 / 255.0;
            return brightness > 0.5 ? System.Windows.Media.Color.FromRgb(102, 102, 102) : System.Windows.Media.Color.FromRgb(128, 128, 128);
        }

        private System.Windows.Media.Color GenerateHoverColor(System.Windows.Media.Color primary)
        {
            var brightness = (primary.R + primary.G + primary.B) / 3.0 / 255.0;
            return brightness > 0.5 ? System.Windows.Media.Color.FromRgb(224, 224, 224) : System.Windows.Media.Color.FromRgb(68, 68, 68);
        }

        private System.Windows.Media.Color GeneratePressedColor(System.Windows.Media.Color primary)
        {
            var brightness = (primary.R + primary.G + primary.B) / 3.0 / 255.0;
            return brightness > 0.5 ? System.Windows.Media.Color.FromRgb(208, 208, 208) : System.Windows.Media.Color.FromRgb(102, 102, 102);
        }
    }

    public class ThemeChangedEventArgs : EventArgs
    {
        public ThemeService.Theme NewTheme { get; }

        public ThemeChangedEventArgs(ThemeService.Theme newTheme)
        {
            NewTheme = newTheme;
        }
    }
}