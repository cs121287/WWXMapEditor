using System;
using System.Globalization;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    /// <summary>
    /// Clamps a numeric value to [Min, Max] after multiplying by MultiplyMultiplier.
    /// ConverterParameter can override as "min,max".
    /// </summary>
    public class ClampDoubleConverter : IValueConverter
    {
        public double Min { get; set; } = 0;
        public double Max { get; set; } = double.MaxValue;
        public double MultiplyMultiplier { get; set; } = 1.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!TryGetDouble(value, culture, out var d))
                return Min;

            d *= MultiplyMultiplier;

            if (parameter is string p && p.Contains(','))
            {
                var parts = p.Split(',');
                if (parts.Length >= 2 &&
                    double.TryParse(parts[0], NumberStyles.Any, culture, out var pMin) &&
                    double.TryParse(parts[1], NumberStyles.Any, culture, out var pMax))
                {
                    return Math.Clamp(d, pMin, pMax);
                }
            }

            return Math.Clamp(d, Min, Max);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        private bool TryGetDouble(object input, CultureInfo culture, out double result)
        {
            if (input is double dd) { result = dd; return true; }
            if (input == null) { result = 0; return false; }
            return double.TryParse(input.ToString(), NumberStyles.Any, culture, out result);
        }
    }
}