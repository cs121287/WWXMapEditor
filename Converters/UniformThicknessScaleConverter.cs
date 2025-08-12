using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    /// <summary>
    /// Uniform thickness = Base * scale for all sides. ConverterParameter overrides Base.
    /// </summary>
    public class UniformThicknessScaleConverter : IValueConverter
    {
        public double Base { get; set; } = 8;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double scale = 1.0;
            if (value is double d) scale = d;
            else if (value != null && double.TryParse(value.ToString(), NumberStyles.Any, culture, out var parsed))
                scale = parsed;

            double b = Base;
            if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Any, culture, out var p))
                b = p;

            var v = Math.Round(b * scale);
            return new Thickness(v);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}