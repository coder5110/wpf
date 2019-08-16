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
    public class NameValidationRule: ValidationRule
    {
        #region Methods

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult res = new ValidationResult(true, null);
            //Regex regex = new Regex(@"^[a-zA-Z ]+$");
            //string str = value as string;

            //if (!string.IsNullOrEmpty(str) && !regex.IsMatch(str))
            //{
            //    res = new ValidationResult(false, "Must contains only letters and spaces");
            //}

            return res;
        }

        #endregion
    }
}
