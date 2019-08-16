using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bot.ValidationRules
{
    class NotNullValidationRule: ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult res = new ValidationResult(true, null);

            if (value == null)
            {
                res = new ValidationResult(false, "This field is required");
            }

            return res;
        }
    }
}
