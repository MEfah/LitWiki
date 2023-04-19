using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace LitWiki.Tools
{
    public enum BooleanToVisibilityConversionMode
    {
        FalseIsCollapsed,
        FalseIsHidden,
        TrueIsCollapsed,
        TrueIsHidden
    }

    [ValueConversion(typeof(bool), typeof(Visibility), ParameterType = typeof(BooleanToVisibilityConversionMode))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
    System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("Target must be a Visibility enum");
            bool val = (bool)value;

            if (parameter == null)
                return val ? Visibility.Visible : Visibility.Collapsed;

            if (parameter is not BooleanToVisibilityConversionMode mode)
                throw new InvalidOperationException("Parameter must be a BoolenToVisibilityConversionMode enum");

            switch (mode)
            {
                case BooleanToVisibilityConversionMode.FalseIsHidden:
                    return val ? Visibility.Visible : Visibility.Hidden;

                case BooleanToVisibilityConversionMode.TrueIsCollapsed:
                    return val ? Visibility.Collapsed : Visibility.Visible;

                case BooleanToVisibilityConversionMode.TrueIsHidden:
                    return val ? Visibility.Hidden : Visibility.Collapsed;

                default:
                    return val ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");
            Visibility val = (Visibility)value;

            if (parameter == null)
                return val != Visibility.Collapsed;

            if (parameter is not BooleanToVisibilityConversionMode mode)
                throw new InvalidOperationException("Parameter must be a BoolenToVisibilityConversionMode enum");

            switch (mode)
            {
                case BooleanToVisibilityConversionMode.FalseIsHidden:
                    return val != Visibility.Hidden;

                case BooleanToVisibilityConversionMode.TrueIsCollapsed:
                    return val == Visibility.Collapsed;

                case BooleanToVisibilityConversionMode.TrueIsHidden:
                    return val == Visibility.Hidden;

                default:
                    return val != Visibility.Collapsed;
            }
        }
    }
}
