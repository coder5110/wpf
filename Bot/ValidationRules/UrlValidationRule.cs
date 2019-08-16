using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bot.ValidationRules
{
    class UrlValidationRule: ValidationRule
    {
        #region Methods

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult res = new ValidationResult(true, null);
            string str = value as string;

            if (!string.IsNullOrEmpty(str))
            {
                Uri uriResult;
                if (!(Uri.TryCreate(str, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)))
                {
                    res = new ValidationResult(false, "Wrong format");
                }
            }

            return res;
        }

        #endregion
    }
}
