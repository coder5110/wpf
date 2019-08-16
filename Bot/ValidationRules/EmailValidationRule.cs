using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bot.ValidationRules
{
    class EmailValidationRule: ValidationRule
    {
        #region Methods

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult res = new ValidationResult(true, null);
            string str = value as string;
            bool same = false;

            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    MailAddress mail = new MailAddress(str);
                    same = mail.Address == str;

                }
                catch (Exception e)
                {
                    
                }
            }

            if (!same)
            {
                res = new ValidationResult(false, "Not valid format");
            }

            return res;
        }

        #endregion
    }
}
