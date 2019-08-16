using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bot.ValidationRules
{
    public class RequiredValidationRule: ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = value as string;
            ValidationResult res = new ValidationResult(true, null);

            if (string.IsNullOrEmpty(str))
            {
                res = new ValidationResult(false, "This field is required");
            }

            return res;
        }
    }
}
