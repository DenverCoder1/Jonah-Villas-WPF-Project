using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPFPL
{
    public class Normalize : IValueConverter
    {
        /// <summary>
        /// Normalize string for searches
        /// </summary>
        public static string Convert(object value)
        {
            if (value == null) return "";
            return value.ToString().ToLower().Replace(" ","");
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
