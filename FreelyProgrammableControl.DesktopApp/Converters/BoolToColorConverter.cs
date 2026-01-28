using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace FreelyProgrammableControl.DesktopApp.Converters
{
    /// <summary>
    /// Converts a boolean value to a background color (Green for true, Red for false).
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Transparent);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}
