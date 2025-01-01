using System;
using System.Globalization;
using System.Windows.Data;

namespace NeatShift.Views
{
    public class BooleanToArrowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value 
                ? "M0,4 L8,4 L4,0 Z"  // Up arrow
                : "M0,0 L8,0 L4,4 Z"; // Down arrow
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 