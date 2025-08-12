using System;
using System.Globalization;
using System.Windows.Data;
using WWXMapEditor.UI.Scaling;

namespace WWXMapEditor.Converters
{
    /// <summary>
    /// Returns different double constants based on LayoutDensity (e.g. for gap, corner radius).
    /// </summary>
    public class LayoutDensityToDoubleConverter : IValueConverter
    {
        public double Compact { get; set; } = 4;
        public double Normal { get; set; } = 8;
        public double Spacious { get; set; } = 12;

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