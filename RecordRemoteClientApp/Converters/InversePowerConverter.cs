using RecordRemoteClientApp.Enumerations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RecordRemoteClientApp.Converters
{
    public class InversePowerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value as PowerStatus?)
            { 
                case PowerStatus.On:
                    return "Switch " + PowerStatus.Off;
                case PowerStatus.Off:
                    return "Switch " + PowerStatus.On;
                default:
                    return PowerStatus.Unknown;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
