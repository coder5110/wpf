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
    //public class ShoeSizeDecimalToStringConverter: IValueConverter
    //{
    //    #region Methods

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (targetType != typeof(string))
    //        {
    //            throw new InvalidOperationException("TargetType must be string");
    //        }

    //        decimal size = (decimal) value;

    //        return size == ShoeSizeCollection.RandomSize ? "RANDOM" : size.ToString();
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (targetType != typeof(decimal))
    //        {
    //            throw new InvalidOperationException("TargetType must be decimal");
    //        }

    //        string str = value as string;

    //        if (str == null)
    //        {
    //            return DependencyProperty.UnsetValue;
    //        }

    //        return str == "RANDOM" ? ShoeSizeCollection.RandomSize : decimal.Parse(str);
    //    }

    //    #endregion
    //}
}
