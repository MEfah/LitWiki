using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LitWiki.Tools
{
    class RatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is null)
                return value;

            var val = (int)value;
            var param = int.Parse(parameter.ToString());

            return val * param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is null)
                return value;

            var val = (int)value;
            var param = int.Parse(parameter.ToString());

            if(param != 0)
                return val / param;

            return 0;
        }
    }
}
