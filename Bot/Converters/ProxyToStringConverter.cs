using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Bot.Models;

namespace Bot.Converters
{
    [ValueConversion(typeof(Proxy), typeof(string))]
    public class ProxyToStringConverter: IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("TargetType must be string");
            }

            Proxy proxy = value as Proxy;

            if (proxy == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return proxy.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Proxy))
            {
                throw new InvalidOperationException("TargetType must be Proxy");
            }

            string str = value as string;

            if (str == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return str != "" ? new Proxy(str) : null;
        }

        #endregion
    }
}
