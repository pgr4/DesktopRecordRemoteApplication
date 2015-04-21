using RecordRemoteClientApp.Enumerations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace RecordRemoteClientApp.Converters
{
    /// <summary>
    /// Based on the status returns a color
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var t = value.GetType();
            if (t == typeof(BusyStatus))
            {
                switch (value as BusyStatus?)
                {
                    case BusyStatus.Ready:
                        return new SolidColorBrush(Colors.Green);
                    case BusyStatus.Unknown:
                        return new SolidColorBrush(Colors.Red);
                    default:
                        return new SolidColorBrush(Colors.Orange);
                }
            }
            else if(t == typeof(PowerStatus))
            {
                switch (value as PowerStatus?)
                {
                    case PowerStatus.On:
                        return new SolidColorBrush(Colors.Green);
                    default:
                        return new SolidColorBrush(Colors.Red);
                }
            }


            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
