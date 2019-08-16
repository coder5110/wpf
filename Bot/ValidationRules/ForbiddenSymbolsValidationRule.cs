using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bot.ValidationRules
{
    class ForbiddenSymbolsValidationRule: ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult res = new ValidationResult(true, null);
            string str = value as string;

            if (!string.IsNullOrEmpty(str))
            {
                if (str.Contains(";"))
                {
                    res = new ValidationResult(false, "Symbol ';' is forbidden");
                }
            }

            return res;
        }
    }
}
