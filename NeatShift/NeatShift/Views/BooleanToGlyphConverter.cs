using System;
using System.Globalization;
using System.Windows.Data;

namespace NeatShift.Views
{
    public class BooleanToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string glyphs)
            {
                var parts = glyphs.Split(',');
                if (parts.Length == 2)
                {
                    return boolValue ? parts[1] : parts[0];
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 