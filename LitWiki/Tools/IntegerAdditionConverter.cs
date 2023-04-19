using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.CodeDom;
using System.Diagnostics;

namespace LitWiki.Tools
{
    public class IntegerAdditionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is null)
                return value;

            var val = (int)value;
            var param = int.Parse(parameter.ToString());

            return val + param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is null)
                return value;

            var val = (int)value;
            var param = int.Parse(parameter.ToString());

            return val - param;
        }
    }
}
