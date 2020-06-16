using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Project01_3693_dotNet5780
{
    public class AddMonthsConverter : IValueConverter
    {
        /// <summary>
        /// Add 11 months to passed date
        /// </summary>
        public static string Convert(object value)
        {
            if (value != null && value.ToString() is string d)
            {
                if (DateTime.TryParse(d, out DateTime dt))
                {
                    return dt.AddMonths(11).ToString();
                }
                return "";
            }
            return "";
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
