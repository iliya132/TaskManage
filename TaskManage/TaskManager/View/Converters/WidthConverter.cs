using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskManager.View.Converters
{
    public class WidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                return (double)value > 40 ? (double)value - 40 : 100;
            }
            else
            {
                throw new FormatException("Поступило некорректное значение");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                return (double)value > 40 ? (double)value + 40 : 100;
            }
            else
            {
                throw new FormatException("Поступило некорректное значение");
            }
        }
    }

    public class WidthConverterForText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                return (double)value > 40 ? (double)value - 100 : 100;
            }
            else
            {
                throw new FormatException("Поступило некорректное значение");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                return (double)value > 40 ? (double)value + 100 : 100;
            }
            else
            {
                throw new FormatException("Поступило некорректное значение");
            }
        }
    }
}
