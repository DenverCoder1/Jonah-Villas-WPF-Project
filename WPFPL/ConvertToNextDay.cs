using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Project01_3693_dotNet5780
{
    public class ConvertToNextDay : IValueConverter
    {
        /// <summary>
        /// Add 1 day to passed date
        /// If error in value, return tomorrow's date
        /// </summary>
        public static string Convert(object value)
        {
            if (value != null && value.ToString() is string d)
            {
                if (DateTime.TryParse(d, out DateTime dt))
                {
                    // add 1 day to passed date
                    return dt.AddDays(1).ToString();
                }
                // return tomorrow
                return DateTime.Today.AddDays(1).ToString();
            }
            // return tomorrow
            return DateTime.Today.AddDays(1).ToString();
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
