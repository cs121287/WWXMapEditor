using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WWXMapEditor.UI.Scaling;

namespace WWXMapEditor.Converters
{
    /// <summary>
    /// Maps LayoutDensity enum to different Thickness values.
    /// </summary>
    public class LayoutDensityToThicknessConverter : IValueConverter
    {
        public Thickness Compact { get; set; } = new Thickness(4);
        public Thickness Normal { get; set; } = new Thickness(8);
        public Thickness Spacious { get; set; } = new Thickness(12);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LayoutDensity d)
            {
                return d switch
                {
                    LayoutDensity.Compact => Compact,
                    LayoutDensity.Spacious => Spacious,
                    _ => Normal
                };
            }
            return Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}