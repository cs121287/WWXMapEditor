using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WWXMapEditor.UI.Scaling
{
    public class MultiplyConverter : IValueConverter
    {
        public double Factor { get; set; } = 1.0;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d) return d * Factor;
            return Factor;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class ThicknessScaleConverter : IValueConverter
    {
        public double BaseLeft { get; set; }
        public double BaseTop { get; set; }
        public double BaseRight { get; set; }
        public double BaseBottom { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double s)
            {
                return new Thickness(
                    Math.Round(BaseLeft * s),
                    Math.Round(BaseTop * s),
                    Math.Round(BaseRight * s),
                    Math.Round(BaseBottom * s));
            }
            return new Thickness(BaseLeft, BaseTop, BaseRight, BaseBottom);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}