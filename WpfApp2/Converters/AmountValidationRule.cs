using System.Globalization;
using System.Windows.Controls;

namespace WpfApp2.Converters
{
	public class AmountValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			if (value is string s && decimal.TryParse(s, out decimal amount))
			{
				if (amount < 0) return new ValidationResult(false, "—умма не может быть отрицательной");
				if (amount == 0) return new ValidationResult(false, "¬ведите сумму больше 0");
				return ValidationResult.ValidResult;
			}
			return new ValidationResult(false, "¬ведите корректное число");
		}
	}
}