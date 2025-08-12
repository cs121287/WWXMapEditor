using System;
using System.Globalization;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    /// <summary>
    /// Two-way configurable bool <-> index converter.
    /// TrueIndex defaults to 0; FalseIndex defaults to 1.
    /// </summary>
    public class BooleanToIndexTwoWayConverter : IValueConverter
    {
        public int TrueIndex { get; set; } = 0;
        public int FalseIndex { get; set; } = 1;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return b ? TrueIndex : FalseIndex;
            return FalseIndex;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i) return i == TrueIndex;
            if (value != null && int.TryParse(value.ToString(), NumberStyles.Any, culture, out var parsed))
                return parsed == TrueIndex;
            return false;
        }
    }
}