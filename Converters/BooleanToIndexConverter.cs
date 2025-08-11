using System;
using System.Globalization;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    public class BooleanToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDark)
            {
                return isDark ? 0 : 1; // 0 for Dark, 1 for Light
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return index == 0; // true for Dark (index 0), false for Light (index 1)
            }
            return true;
        }
    }
}