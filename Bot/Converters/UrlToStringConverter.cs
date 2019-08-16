using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Bot.Converters
{
    public class UrlToStringConverter: IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("TargetType must be string");
            }

            Url url = value as Url;

            if (url == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return url.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Url))
            {
                throw new InvalidOperationException("TargetType must be Url");
            }

            string str = value as string;

            if (str == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return new Url(str);
        }

        #endregion
    }
}
