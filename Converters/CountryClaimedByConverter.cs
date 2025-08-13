using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    // IMultiValueConverter for tooltip text on a country tile.
    // values:
    //   [0] = item country name (string)
    //   [1] = current player's selection (string)
    //   [2..] = other players' selections (string)
    // Returns:
    //   string "Taken" when some other player has claimed this specific country; null otherwise.
    public sealed class CountryClaimedByConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string item = values.Length > 0 ? values[0]?.ToString() ?? "" : "";
            string current = values.Length > 1 ? values[1]?.ToString() ?? "Random" : "Random";
            var others = values.Skip(2).Select(v => v?.ToString() ?? "Random");

            if (string.IsNullOrWhiteSpace(item) || string.Equals(item, "Random", StringComparison.OrdinalIgnoreCase))
                return null; // No tooltip for Random

            // No tooltip if it's currently selected by this player
            if (string.Equals(item, current, StringComparison.OrdinalIgnoreCase))
                return null;

            // If any other player has this item, show a generic "Taken" tooltip
            foreach (var o in others)
            {
                if (!string.Equals(o, "Random", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(o, item, StringComparison.OrdinalIgnoreCase))
                {
                    return "Taken";
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return Array.Empty<object>();
        }
    }
}