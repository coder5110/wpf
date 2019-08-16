using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Bot.Models;

namespace Bot.Converters
{
    [ValueConversion(typeof(PayCardTypeCode), typeof(string))]
    class PayCardTypeCodeToImageSourceConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("TargetType must be string");
            }

            PayCardTypeCode code = (PayCardTypeCode)value;

            return PayCardTypeCollection.TypesDictionary[code].ImageSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
