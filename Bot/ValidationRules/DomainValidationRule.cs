using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bot.ValidationRules
{
    class DomainValidationRule: ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string ipStr = value as string;
            ValidationResult res = new ValidationResult(true, null);

            if (Uri.CheckHostName(ipStr) == UriHostNameType.Unknown)
            {
                res = new ValidationResult(false, null);
            }

            return res;
        }
    }
}
