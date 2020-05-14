using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TaskManager.View.Converters
{
    public class BooleanToVisibility :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                throw new FormatException("Поступило некорректное значение");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                throw new FormatException("Поступило некорректное значение");
            }
        }
    }
}
