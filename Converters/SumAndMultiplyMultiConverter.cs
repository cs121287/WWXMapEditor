using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    /// <summary>
    /// Sums all numeric MultiBinding values then multiplies by Factor (or ConverterParameter).
    /// </summary>
    public class SumAndMultiplyMultiConverter : IMultiValueConverter
    {
        public double Factor { get; set; } = 1.0;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double localFactor = Factor;
            if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Any, culture, out var p))
                localFactor = p;

            double sum = 0;
            if (values != null)
            {
                foreach (var v in values)
                {
                    if (v is double d) sum += d;
                    else if (v != null && double.TryParse(v.ToString(), NumberStyles.Any, culture, out var parsed))
                        sum += parsed;
                }
            }
            return sum * localFactor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}