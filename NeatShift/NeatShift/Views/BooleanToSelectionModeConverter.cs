using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeatShift.Converters
{
    public class BooleanToSelectionModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? SelectionMode.Multiple : SelectionMode.Single;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (SelectionMode)value == SelectionMode.Multiple;
        }
    }
} 