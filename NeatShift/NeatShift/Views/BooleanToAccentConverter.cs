using System;
using System.Globalization;
using System.Windows.Data;

namespace NeatShift.Converters
{
    public class BooleanToAccentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value 
                ? System.Windows.Application.Current.Resources["SystemAccentBrush"] 
                : System.Windows.Application.Current.Resources["SystemControlForegroundBaseMediumHighBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 