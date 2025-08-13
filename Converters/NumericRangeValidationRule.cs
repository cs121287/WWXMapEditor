using System.Globalization;
using System.Windows.Controls;

namespace WWXMapEditor.Converters
{
    // Validates that input is an integer and MinExclusive < value < MaxExclusive
    public class NumericRangeValidationRule : ValidationRule
    {
        public int MinExclusive { get; set; } = 20;
        public int MaxExclusive { get; set; } = 400;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string ?? value?.ToString() ?? string.Empty;

            if (!int.TryParse(input, NumberStyles.Integer, cultureInfo, out int number))
            {
                return new ValidationResult(false, "Enter a whole number");
            }

            if (number <= MinExclusive || number >= MaxExclusive)
            {
                return new ValidationResult(false, $"Enter a number > {MinExclusive} and < {MaxExclusive}");
            }

            return ValidationResult.ValidResult;
        }
    }
}