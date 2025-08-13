using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace WWXMapEditor.Converters
{
    // Returns the country list filtered so no other player's current selections (except "Random") appear.
    // values[0] = base IEnumerable<string> of all countries (first item can be "Random")
    // values[1] = current player's selection (string)
    // values[2..] = other players' selections (string)
    public class UniqueCountryListConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 1 || values[0] is not IEnumerable baseEnum)
                return Array.Empty<string>();

            var all = baseEnum.Cast<object>().Select(o => o?.ToString() ?? string.Empty).ToList();

            // Current player's selection should remain in the list to avoid clearing UI if it conflicts temporarily
            string current = values.Length > 1 ? values[1]?.ToString() ?? string.Empty : string.Empty;

            // Collect other selections to exclude (non-empty, not "Random")
            var exclude = new HashSet<string>(
                values.Skip(2)
                      .Select(v => v?.ToString())
                      .Where(s => !string.IsNullOrWhiteSpace(s) && !string.Equals(s, "Random", StringComparison.OrdinalIgnoreCase))!
            , StringComparer.OrdinalIgnoreCase);

            // Build filtered list
            var result = new List<string>(all.Count);

            foreach (var c in all)
            {
                if (string.Equals(c, "Random", StringComparison.OrdinalIgnoreCase))
                {
                    // Always include Random
                    result.Add(c);
                    continue;
                }

                // Keep if not selected by others OR it's this player's current selection
                if (!exclude.Contains(c) || string.Equals(c, current, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(c);
                }
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // One-way converter
            throw new NotSupportedException();
        }
    }
}