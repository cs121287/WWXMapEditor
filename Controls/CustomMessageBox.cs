using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace WWXMapEditor.Controls
{
    public enum MessageBoxType
    {
        Information,
        Warning,
        Error,
        Question,
        Success
    }

    public enum MessageBoxButtons
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel,
        RetryCancel
    }

    // Custom MessageBoxResult that includes Retry
    public enum CustomMessageBoxResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Yes = 3,
        No = 4,
        Retry = 5
    }

    public class CustomMessageBox : Window
    {
        private CustomMessageBoxResult _result = CustomMessageBoxResult.None;
        private static readonly object _lock = new object();

        public static CustomMessageBoxResult Show(string message)
        {
            return Show(message, "Message", MessageBoxButtons.OK, MessageBoxType.Information);
        }

        public static CustomMessageBoxResult Show(string message, string title)
        {
            return Show(message, title, MessageBoxButtons.OK, MessageBoxType.Information);
        }

        public static CustomMessageBoxResult Show(string message, string title, MessageBoxButtons buttons)
        {
            return Show(message, title, buttons, MessageBoxType.Information);
        }

        public static CustomMessageBoxResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxType type)
        {
            lock (_lock)
            {
                var messageBox = new CustomMessageBox(message, title, buttons, type);
                messageBox.ShowDialog();
                return messageBox._result;
            }
        }

        private CustomMessageBox(string message, string title, MessageBoxButtons buttons, MessageBoxType type)
        {
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = System.Windows.Application.Current?.MainWindow;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = System.Windows.Media.Brushes.Transparent;
            MinWidth = 400;
            MaxWidth = 600;
            MinHeight = 180;
            SizeToContent = SizeToContent.WidthAndHeight;
            ShowActivated = true;
            Topmost = true;

            // Create main content
            var mainGrid = new Grid();
            mainGrid.Margin = new Thickness(20);

            // Main border with shadow
            var mainBorder = new Border
            {
                Background = System.Windows.Application.Current.FindResource("BackgroundBrush") as System.Windows.Media.Brush ?? new SolidColorBrush(Colors.White),
                BorderBrush = System.Windows.Application.Current.FindResource("BorderBrush") as System.Windows.Media.Brush ?? new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };

            // Add shadow effect
            mainBorder.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 270,
                ShadowDepth = 5,
                Opacity = 0.3,
                BlurRadius = 20
            };

            var contentGrid = new Grid();
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 60 });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Header
            var headerBorder = new Border
            {
                Background = GetHeaderBrush(type),
                CornerRadius = new CornerRadius(8, 8, 0, 0)
            };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Icon
            var iconBorder = new Border
            {
                Width = 40,
                Height = 40,
                Margin = new Thickness(15, 10, 10, 10)
            };

            var iconTextBlock = new TextBlock
            {
                Text = GetIcon(type),
                FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
                FontSize = 24,
                Foreground = System.Windows.Media.Brushes.White,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconBorder.Child = iconTextBlock;
            Grid.SetColumn(iconBorder, 0);
            headerGrid.Children.Add(iconBorder);

            // Title
            var titleTextBlock = new TextBlock
            {
                Text = title,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(titleTextBlock, 1);
            headerGrid.Children.Add(titleTextBlock);

            // Close button
            var closeButton = new System.Windows.Controls.Button
            {
                Content = "✕",
                FontSize = 16,
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                Foreground = System.Windows.Media.Brushes.White,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Width = 40,
                Height = 40,
                Cursor = System.Windows.Input.Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            closeButton.Click += (s, e) =>
            {
                _result = CustomMessageBoxResult.Cancel;
                Close();
            };

            // Close button hover effect
            closeButton.MouseEnter += (s, e) =>
            {
                closeButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 255, 255));
            };
            closeButton.MouseLeave += (s, e) =>
            {
                closeButton.Background = System.Windows.Media.Brushes.Transparent;
            };

            Grid.SetColumn(closeButton, 2);
            headerGrid.Children.Add(closeButton);

            headerBorder.Child = headerGrid;
            Grid.SetRow(headerBorder, 0);
            contentGrid.Children.Add(headerBorder);

            // Message content
            var messageBorder = new Border
            {
                Padding = new Thickness(20)
            };

            var messageScrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = 300
            };

            var messageTextBlock = new TextBlock
            {
                Text = message,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 14,
                Foreground = System.Windows.Application.Current.FindResource("ForegroundBrush") as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.Black,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 22
            };

            messageScrollViewer.Content = messageTextBlock;
            messageBorder.Child = messageScrollViewer;
            Grid.SetRow(messageBorder, 1);
            contentGrid.Children.Add(messageBorder);

            // Buttons panel
            var buttonsBorder = new Border
            {
                Background = System.Windows.Application.Current.FindResource("SurfaceBrush") as System.Windows.Media.Brush ?? new SolidColorBrush(Colors.LightGray),
                BorderBrush = System.Windows.Application.Current.FindResource("BorderBrush") as System.Windows.Media.Brush ?? new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(0, 1, 0, 0),
                CornerRadius = new CornerRadius(0, 0, 8, 8),
                Padding = new Thickness(15)
            };

            var buttonsPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right
            };

            AddButtons(buttonsPanel, buttons, type);
            buttonsBorder.Child = buttonsPanel;
            Grid.SetRow(buttonsBorder, 2);
            contentGrid.Children.Add(buttonsBorder);

            mainBorder.Child = contentGrid;
            mainGrid.Children.Add(mainBorder);
            Content = mainGrid;

            // Keyboard shortcuts
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    _result = CustomMessageBoxResult.Cancel;
                    Close();
                }
                else if (e.Key == Key.Enter && buttons == MessageBoxButtons.OK)
                {
                    _result = CustomMessageBoxResult.OK;
                    Close();
                }
            };

            // Animation on load
            Loaded += (s, e) =>
            {
                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                var scaleX = new DoubleAnimation
                {
                    From = 0.9,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                var scaleY = new DoubleAnimation
                {
                    From = 0.9,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                var scaleTransform = new ScaleTransform(0.9, 0.9);
                mainGrid.RenderTransform = scaleTransform;
                mainGrid.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                BeginAnimation(OpacityProperty, fadeIn);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
            };
        }

        private System.Windows.Media.Brush GetHeaderBrush(MessageBoxType type)
        {
            return type switch
            {
                MessageBoxType.Error => new LinearGradientBrush(
                    System.Windows.Media.Color.FromRgb(220, 53, 69),
                    System.Windows.Media.Color.FromRgb(200, 35, 51),
                    90),
                MessageBoxType.Warning => new LinearGradientBrush(
                    System.Windows.Media.Color.FromRgb(255, 193, 7),
                    System.Windows.Media.Color.FromRgb(255, 173, 0),
                    90),
                MessageBoxType.Success => new LinearGradientBrush(
                    System.Windows.Media.Color.FromRgb(40, 167, 69),
                    System.Windows.Media.Color.FromRgb(30, 150, 59),
                    90),
                MessageBoxType.Question => new LinearGradientBrush(
                    System.Windows.Media.Color.FromRgb(0, 123, 255),
                    System.Windows.Media.Color.FromRgb(0, 103, 235),
                    90),
                _ => System.Windows.Application.Current.FindResource("AccentBrush") as System.Windows.Media.Brush ??
                     new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 215))
            };
        }

        private string GetIcon(MessageBoxType type)
        {
            return type switch
            {
                MessageBoxType.Error => "\uE783",      // Error icon
                MessageBoxType.Warning => "\uE7BA",    // Warning icon
                MessageBoxType.Success => "\uE73E",    // Checkmark icon
                MessageBoxType.Question => "\uE897",   // Question icon
                _ => "\uE946"                          // Info icon
            };
        }

        private void AddButtons(System.Windows.Controls.Panel panel, MessageBoxButtons buttons, MessageBoxType type)
        {
            var buttonStyle = System.Windows.Application.Current.FindResource("SecondaryButtonStyle") as Style;
            var primaryButtonStyle = System.Windows.Application.Current.FindResource("PrimaryButtonStyle") as Style;

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    AddButton(panel, "OK", CustomMessageBoxResult.OK, primaryButtonStyle, true);
                    break;

                case MessageBoxButtons.OKCancel:
                    AddButton(panel, "Cancel", CustomMessageBoxResult.Cancel, buttonStyle, false);
                    AddButton(panel, "OK", CustomMessageBoxResult.OK, primaryButtonStyle, true);
                    break;

                case MessageBoxButtons.YesNo:
                    AddButton(panel, "No", CustomMessageBoxResult.No, buttonStyle, false);
                    AddButton(panel, "Yes", CustomMessageBoxResult.Yes, primaryButtonStyle, true);
                    break;

                case MessageBoxButtons.YesNoCancel:
                    AddButton(panel, "Cancel", CustomMessageBoxResult.Cancel, buttonStyle, false);
                    AddButton(panel, "No", CustomMessageBoxResult.No, buttonStyle, false);
                    AddButton(panel, "Yes", CustomMessageBoxResult.Yes, primaryButtonStyle, true);
                    break;

                case MessageBoxButtons.RetryCancel:
                    AddButton(panel, "Cancel", CustomMessageBoxResult.Cancel, buttonStyle, false);
                    AddButton(panel, "Retry", CustomMessageBoxResult.Retry, primaryButtonStyle, true);
                    break;
            }
        }

        private void AddButton(System.Windows.Controls.Panel panel, string text, CustomMessageBoxResult result, Style? style, bool isDefault)
        {
            var button = new System.Windows.Controls.Button
            {
                Content = text,
                Style = style,
                MinWidth = 90,
                Margin = new Thickness(5, 0, 0, 0),
                IsDefault = isDefault
            };

            button.Click += (s, e) =>
            {
                _result = result;

                // Fade out animation before closing
                var fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(150)
                };

                fadeOut.Completed += (sender, args) => Close();
                BeginAnimation(OpacityProperty, fadeOut);
            };

            panel.Children.Add(button);
        }
    }

    // Extension methods to convert between CustomMessageBoxResult and MessageBoxResult
    public static class MessageBoxResultExtensions
    {
        public static MessageBoxResult ToStandardResult(this CustomMessageBoxResult customResult)
        {
            return customResult switch
            {
                CustomMessageBoxResult.OK => MessageBoxResult.OK,
                CustomMessageBoxResult.Cancel => MessageBoxResult.Cancel,
                CustomMessageBoxResult.Yes => MessageBoxResult.Yes,
                CustomMessageBoxResult.No => MessageBoxResult.No,
                _ => MessageBoxResult.None
            };
        }

        public static CustomMessageBoxResult ToCustomResult(this MessageBoxResult standardResult)
        {
            return standardResult switch
            {
                MessageBoxResult.OK => CustomMessageBoxResult.OK,
                MessageBoxResult.Cancel => CustomMessageBoxResult.Cancel,
                MessageBoxResult.Yes => CustomMessageBoxResult.Yes,
                MessageBoxResult.No => CustomMessageBoxResult.No,
                _ => CustomMessageBoxResult.None
            };
        }
    }
}