using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bot.ValidationRules
{
    public class NumericValidationRule: ValidationRule
    {
        #region Properties

        public int Min { get; set; }
        public int Max { get; set; }

        #endregion

        #region Methods

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = value as string;
            int port;
            ValidationResult res = new ValidationResult(true, null);

            if (!int.TryParse(str, out port))
            {
                res = new ValidationResult(false, "Wrong port format");
            }
            else if (port < Min || port > Max)
            {
                res = new ValidationResult(false, $"Value must be in range [{Min} .. {Max}]");
            }

            return res;
        }

        #endregion
    }
}
