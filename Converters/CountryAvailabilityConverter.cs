using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    // IMultiValueConverter for enabling/disabling a country tile.
    // values:
    //   [0] = item country name (string)
    //   [1] = current player's selection (string)
    //   [2..] = other players' selections (string)
    public sealed class CountryAvailabilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string item = values.Length > 0 ? values[0]?.ToString() ?? "" : "";
            string current = values.Length > 1 ? values[1]?.ToString() ?? "Random" : "Random";
            var others = values.Skip(2).Select(v => v?.ToString() ?? "Random");

            if (string.Equals(item, "Random", StringComparison.OrdinalIgnoreCase))
                return true; // "Random" is always available

            // If this item is the currently selected value for this player, allow it
            if (string.Equals(item, current, StringComparison.OrdinalIgnoreCase))
                return true;

            // If any other player has already chosen this item (non-Random), disable it
            foreach (var o in others)
            {
                if (!string.Equals(o, "Random", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(o, item, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return Array.Empty<object>();
        }
    }
}