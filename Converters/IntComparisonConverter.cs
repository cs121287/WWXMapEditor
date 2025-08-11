using System;
using System.Globalization;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    public class IntComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            if (int.TryParse(value.ToString(), out int intValue) && 
                int.TryParse(parameter.ToString(), out int intParameter))
            {
                return intValue == intParameter;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter != null)
            {
                if (int.TryParse(parameter.ToString(), out int result))
                {
                    return result;
                }
            }
            return null;
        }
    }
}