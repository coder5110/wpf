using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bot.ValidationRules
{
    class PhoneNumberValidationRule: ValidationRule
    {
        #region Methods

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult res = new ValidationResult(true, null);
            string str = value as string;

            if (!string.IsNullOrEmpty(str))
            {
                OnlyNumbersValidationRule numbersValidationRule = new OnlyNumbersValidationRule();

                res = numbersValidationRule.Validate(str, cultureInfo);

                if (res.IsValid)
                {
                    if (str.Length < 10 || str.Length > 11)
                    {
                        res = new ValidationResult(false, "Phone number must contain 10-11 digitis");
                    }
                }
            }

            return res;
        }

        #endregion
    }
}
