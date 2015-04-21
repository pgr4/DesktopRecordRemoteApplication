using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RecordRemoteClientApp.Converters
{  
    /// <summary>
    /// For Association screen
    /// Converts button text based on amount in list
    /// </summary>
    public class ButtonTextConverter : IValueConverter
    { 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == "Merge")
            {
                return "Abort";
            }
            else
            {
                return "Merge";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
