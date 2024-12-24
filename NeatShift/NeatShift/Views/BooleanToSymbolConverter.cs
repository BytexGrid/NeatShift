using System;
using System.Globalization;
using System.Windows.Data;
using ModernWpf.Controls;

namespace NeatShift.Views
{
    public class BooleanToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string symbols)
            {
                var parts = symbols.Split(',');
                if (parts.Length == 2)
                {
                    return Enum.Parse(typeof(Symbol), boolValue ? parts[1] : parts[0]);
                }
            }
            return Symbol.Add;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 