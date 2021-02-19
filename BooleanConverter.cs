using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace NewsBuddy
{
    public class BooleanConverter<T> : IValueConverter
    {
        // I stole this from StackOverflow, thanks to Atif Aziz.
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }

    }

    public sealed class BoolToVisInverter : BooleanConverter<Visibility>
    {
        public BoolToVisInverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }
}
