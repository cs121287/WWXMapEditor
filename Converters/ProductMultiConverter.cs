using System;
using System.Globalization;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    /// <summary>
    /// Multiplies all numeric MultiBinding values and applies AdditionalFactor (or ConverterParameter).
    /// Non-numeric values ignored if IgnoreNonNumeric = true, otherwise returns 0 on first non-numeric.
    /// </summary>
    public class ProductMultiConverter : IMultiValueConverter
    {
        public bool IgnoreNonNumeric { get; set; } = true;
        public double AdditionalFactor { get; set; } = 1.0;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double product = 1.0;
            if (values != null)
            {
                foreach (var v in values)
                {
                    if (v is double d) product *= d;
                    else if (v != null && double.TryParse(v.ToString(), NumberStyles.Any, culture, out var parsed))
                        product *= parsed;
                    else if (!IgnoreNonNumeric)
                        return 0d;
                }
            }

            double factor = AdditionalFactor;
            if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Any, culture, out var p))
                factor = p;

            return product * factor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}