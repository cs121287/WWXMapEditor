using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WwXMapEditor.Converters
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorString)
            {
                switch (colorString.ToLower())
                {
                    case "blue": return Colors.Blue;
                    case "red": return Colors.Red;
                    case "green": return Colors.Green;
                    case "yellow": return Colors.Yellow;
                    case "orange": return Colors.Orange;
                    case "purple": return Colors.Purple;
                    case "cyan": return Colors.Cyan;
                    case "pink": return Colors.Pink;
                    default: return Colors.Gray;
                }
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}