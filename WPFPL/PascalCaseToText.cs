using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Project01_3693_dotNet5780
{
    public class PascalCaseToText : IValueConverter
    {
        /// <summary>
        /// Function for adding spaces to enum values
        /// Convert string in format "HelloWorld" to "Hello World"
        /// </summary>
        public static string Convert(object value)
        {
            string enumString = value.ToString();
            return Regex.Replace(enumString, "([a-z])([A-Z])", "$1 $2");
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