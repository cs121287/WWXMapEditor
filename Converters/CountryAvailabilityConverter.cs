using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    // Returns true if the candidate country is available for the current player.
    // values[0] = candidate country (string)
    // values[1] = current player's selected country (string)
    // values[2..] = other players' selected countries (string)
    // Rules:
    // - "Random" is always available
    // - A country selected by another player is unavailable
    // - The current player's own selection remains enabled (so UI doesn't clear on refresh)
    public class CountryAvailabilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string candidate = values.Length > 0 ? values[0]?.ToString() ?? string.Empty : string.Empty;
            string current = values.Length > 1 ? values[1]?.ToString() ?? string.Empty : string.Empty;

            if (string.Equals(candidate, "Random", StringComparison.OrdinalIgnoreCase))
                return true;

            var others = values.Skip(2)
                               .Select(v => v?.ToString())
                               .Where(s => !string.IsNullOrWhiteSpace(s) && !string.Equals(s, "Random", StringComparison.OrdinalIgnoreCase));

            bool takenByOther = others.Any(s => string.Equals(s, candidate, StringComparison.OrdinalIgnoreCase));

            if (!takenByOther) return true;

            // Keep enabled if it's the current player's existing choice
            return string.Equals(candidate, current, StringComparison.OrdinalIgnoreCase);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}