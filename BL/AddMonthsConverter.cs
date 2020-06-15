using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BL
{
    public class AddMonthsConverter : IValueConverter
    {
        public static string Convert(object value)
        {
            if (value != null && value.ToString() is string d)
            {
                if (DateTime.TryParse(d, out DateTime dt))
                {
                    string Converted = dt.AddMonths(11).ToString();
                    return Converted;
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
