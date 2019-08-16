using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Bot.Services;

namespace Bot.Converters
{
    [ValueConversion(typeof(SiteProxyTestStatus), typeof(Brush))]
    public class SiteProxyTestStatusToColor: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException("Target type must be Brush");
            }

            SiteProxyTestStatus status = (SiteProxyTestStatus)value;

            return status == SiteProxyTestStatus.Passed
                ? Brushes.Green
                : status == SiteProxyTestStatus.Failed
                    ? Brushes.Red
                    : Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
