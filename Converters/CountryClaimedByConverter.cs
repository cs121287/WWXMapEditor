using System;
using System.Globalization;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    // Returns a friendly "Taken by Player N" string if the given country is already claimed by another player.
    // values[0] = candidate country
    // values[1] = P1 selection
    // values[2] = P2 selection
    // values[3] = P3 selection
    // values[4] = P4 selection
    public class CountryClaimedByConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 5)
                return null;

            string candidate = values[0]?.ToString();
            if (string.IsNullOrWhiteSpace(candidate) || string.Equals(candidate, "Random", StringComparison.OrdinalIgnoreCase))
                return null;

            string p1 = values[1]?.ToString();
            string p2 = values[2]?.ToString();
            string p3 = values[3]?.ToString();
            string p4 = values[4]?.ToString();

            if (string.Equals(candidate, p1, StringComparison.OrdinalIgnoreCase)) return "Taken by Player 1";
            if (string.Equals(candidate, p2, StringComparison.OrdinalIgnoreCase)) return "Taken by Player 2";
            if (string.Equals(candidate, p3, StringComparison.OrdinalIgnoreCase)) return "Taken by Player 3";
            if (string.Equals(candidate, p4, StringComparison.OrdinalIgnoreCase)) return "Taken by Player 4";

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}