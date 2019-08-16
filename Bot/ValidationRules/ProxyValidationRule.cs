using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Bot.Models;

namespace Bot.ValidationRules
{
    public class ProxyValidationRule: ValidationRule
    {
        #region Methods

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult res = new ValidationResult(true, null);
            string str = value as string;

            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    new Proxy(str);
                }
                catch (Exception e)
                {
                    res = new ValidationResult(false, "Wrong format");
                }
            }

            return res;
        }

        #endregion
    }
}
