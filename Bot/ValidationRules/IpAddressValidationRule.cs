using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bot.ValidationRules
{
    class IpAddressValidationRule: ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string ipStr = value as string;
            IPAddress ip = null;
            ValidationResult res = new ValidationResult(true, null);

            if (ipStr.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length != 4 || !IPAddress.TryParse(ipStr, out ip))
            {
                res = new ValidationResult(false, "Wrong format");
            }
            //if (Uri.CheckHostName(ipStr) == UriHostNameType.Unknown)
            //{
            //    res = new ValidationResult(false, null);
            //}
            return res;
        }
    }
}
