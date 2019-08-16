using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bot.ValidationRules
{
    public class OnlyNumbersValidationRule: ValidationRule
    {
        #region Methods

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult res = new ValidationResult(true, null);
            Regex regex = new Regex(@"^\d+$");
            string str = value as string;

            if (!string.IsNullOrEmpty(str) && !regex.IsMatch(str))
            {
                res = new ValidationResult(false, "Must contains only numbers");
            }

            return res;
        }

        #endregion

    }
}
