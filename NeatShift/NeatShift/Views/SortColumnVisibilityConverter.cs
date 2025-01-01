using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeatShift.Views
{
    public class SortColumnVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string sortColumn && parameter is string columnName)
            {
                return sortColumn == columnName ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 