using System;
using System.Globalization;
using System.Runtime.Versioning;
using System.Windows.Data;
using ModernWpf.Controls;

namespace NeatShift.Converters
{
    [SupportedOSPlatform("windows7.0")]
    public class FileTypeToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDirectory)
            {
                return isDirectory ? Symbol.Folder : Symbol.Document;
            }
            return Symbol.Document;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 