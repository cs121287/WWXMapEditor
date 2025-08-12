using System;
using System.Globalization;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    /// <summary>
    /// Multiplies the input numeric value by a Factor (or ConverterParameter override).
    /// Used for adaptive font sizing and size scaling.
    /// </summary>
    public class MultiplyConverter : IValueConverter
    {
        public double Factor { get; set; } = 1.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double factor = Factor;
            if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Any, culture, out var p))
                factor = p;

            if (value is double d) return d * factor;
            if (value == null) return 0d;

            if (double.TryParse(value.ToString(), NumberStyles.Any, culture, out var parsed))
                return parsed * factor;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}