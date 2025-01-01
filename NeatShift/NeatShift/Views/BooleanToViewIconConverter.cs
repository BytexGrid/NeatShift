using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace NeatShift.Views
{
    public class BooleanToViewIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "\uF0E2" : "\uEA37";  // GridView : List
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 