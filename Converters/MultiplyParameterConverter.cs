using System;
using System.Globalization;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    /// <summary>
    /// Lightweight multiply converter that only uses the ConverterParameter as the factor.
    /// </summary>
    public class MultiplyParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double d))
            {
                if (value == null) return 0d;
                if (!double.TryParse(value.ToString(), NumberStyles.Any, culture, out d))
                    return 0d;
            }

            double factor = 1.0;
            if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Any, culture, out var p))
                factor = p;

            return d * factor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}