using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RecordRemoteClientApp.Converters
{
    class AssociationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null && ((int)value == 1 || (int)value == 2))
            {
                return Visibility.Visible;
            }
            else if (parameter == null)
            {
                return Visibility.Collapsed;
            }

            if (int.Parse((string)parameter) == -1 && (int)value >= 10)
            {
                return Visibility.Visible;
            }

            if ((int)value == int.Parse((string)parameter))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
