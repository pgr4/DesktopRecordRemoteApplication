using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RecordRemoteClientApp.Converters
{
    /// <summary>
    /// Converts a boolean to Red or Transparent
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bValue = value as bool?;

            if (bValue.Value)
            {
                return new SolidColorBrush(Colors.Red);
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
