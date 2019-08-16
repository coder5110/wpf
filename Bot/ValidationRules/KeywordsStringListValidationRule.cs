using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Bot.ValidationRules
{
    class KeywordsStringListValidationRule: ValidationRule
    {
        #region DependencyProperties

        //public static readonly DependencyProperty CommaProperty = DependencyProperty.Register(
        //    "Comma", typeof(char), typeof(KeywordsStringListValidationRule), new PropertyMetadata(default(char)));

        //#endregion

        //#region Properties

        //public char Comma
        //{
        //    get { return (char)GetValue(CommaProperty); }
        //    set { SetValue(CommaProperty, value); }
        //}

        #endregion

        #region Methods

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult res = new ValidationResult(true, null);
            string str = value as string;

            if (!string.IsNullOrEmpty(str))
            {
                Regex regex = new Regex(@"^([a-zA-Z0-9_]+\+)*[a-zA-Z0-9_]+$");

                if (!regex.IsMatch(str))
                {
                    res = new ValidationResult(false, "Wrong format");
                }
            }

            return res;
        }

        #endregion

    }
}
