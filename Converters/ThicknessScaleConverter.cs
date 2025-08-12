using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    /// <summary>
    /// Creates a Thickness by scaling base edge values with an input scale factor (double).
    /// Optionally override bases via ConverterParameter "l,t,r,b".
    /// </summary>
    public class ThicknessScaleConverter : IValueConverter
    {
        public double BaseLeft { get; set; }
        public double BaseTop { get; set; }
        public double BaseRight { get; set; }
        public double BaseBottom { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double scale = 1.0;
            if (value is double s) scale = s;
            else if (value != null && double.TryParse(value.ToString(), NumberStyles.Any, culture, out var parsed))
                scale = parsed;

            double left = BaseLeft, top = BaseTop, right = BaseRight, bottom = BaseBottom;

            if (parameter is string param && !string.IsNullOrWhiteSpace(param))
            {
                var parts = param.Split(',');
                if (parts.Length == 4 &&
                    double.TryParse(parts[0], NumberStyles.Any, culture, out var pl) &&
                    double.TryParse(parts[1], NumberStyles.Any, culture, out var pt) &&
                    double.TryParse(parts[2], NumberStyles.Any, culture, out var pr) &&
                    double.TryParse(parts[3], NumberStyles.Any, culture, out var pb))
                {
                    left = pl; top = pt; right = pr; bottom = pb;
                }
            }

            return new Thickness(
                Math.Round(left * scale),
                Math.Round(top * scale),
                Math.Round(right * scale),
                Math.Round(bottom * scale));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}