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
            if (int.Parse((string)parameter) == 3 && (int)value == 50)
            {
                return Visibility.Visible;
            }

            if (parameter == null && ((int)value == 1 || (int)value == 2))
            {
                return Visibility.Visible;
            }
            else if (parameter == null)
            {
                return Visibility.Collapsed;
            }

            if ((string)parameter == "Button" && ((int)value == 2 || (int)value == 3))
            {
                return Visibility.Visible;
            }
            else if ((string)parameter == "Button")
            {
                return Visibility.Collapsed;
            }

            if ((string)parameter == "Label" && (int)value > 0)
            {
                return Visibility.Visible;
            }
            else if ((string)parameter == "Label")
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
