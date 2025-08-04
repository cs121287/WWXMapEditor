using System;
using System.Globalization;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    public class IndexToStepNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return (index + 1).ToString();
            }
            return "1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && int.TryParse(str, out int number))
            {
                return number - 1;
            }
            return 0;
        }
    }
}