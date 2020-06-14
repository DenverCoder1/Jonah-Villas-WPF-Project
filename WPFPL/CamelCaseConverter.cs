﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPFPL
{
    public class CamelCaseConverter : IValueConverter
    {
        public static string Convert(object value)
        {
            if (value != null)
            {
                string enumString = value.ToString();
                string Converted = Regex.Replace(enumString, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
                return Converted;
            }
            return "error";
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