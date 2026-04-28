using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WpfApp2.Models;

namespace WpfApp2.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is Visibility v && v == Visibility.Visible;
    }

    public class AmountToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CategoryType type)
                return type == CategoryType.Income
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A3BE8C"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF616A"));
            return Brushes.Black;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BalanceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal balance)
            {
                if (balance > 0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A3BE8C"));
                if (balance < 0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF616A"));
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C566A"));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}