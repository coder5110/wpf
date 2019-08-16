using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Bot.Converters
{
    public class IpAddressToStringConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("TargetType must be string");
            }

            IPAddress ip = value as IPAddress;

            if (ip == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return ip.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(IPAddress))
            {
                throw new InvalidOperationException("Target type must be IPAddress");
            }

            string str = value as string;
            IPAddress ip;

            if (str == null)
            {
                return DependencyProperty.UnsetValue;
            }

            if (!IPAddress.TryParse(str, out ip))
            {
                return DependencyProperty.UnsetValue;
            }

            return ip;
        }
    }
}
