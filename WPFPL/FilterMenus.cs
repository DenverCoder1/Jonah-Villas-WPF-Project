using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPFPL
{
    public static class FilterMenus
    {
        /// <summary>
        /// Create a menu item given details, return reference to MenuItem
        /// </summary>
        public static MenuItem AddMenuItem(MenuItem parent, string header, bool checkable, string prefix, Action<string, object> RegisterName, RoutedEventHandler MenuItem_Checked)
        {
            if (parent == null) { return parent; }
            // ex. TypeOfPlace => m_typeofplace
            string name = $"m_{prefix}_{Regex.Replace(header, "[^a-zA-Z]", "").ToLower()}";
            // create new menu item
            MenuItem menuItem = new MenuItem
            {
                Name = name,
                Header = PascalCaseToText.Convert(header),
                IsCheckable = checkable,
                IsChecked = checkable
            };
            menuItem.Checked += MenuItem_Checked;
            menuItem.Unchecked += MenuItem_Checked;
            // add item to parent
            parent.Items.Add(menuItem);
            // register name
            RegisterName(name, menuItem);
            return menuItem;
        }

        /// <summary>
        /// Return whether an item is checked based on header
        /// </summary>
        public static bool FilterItemChecked(string key, string prefix, Func<string, MenuItem> FindName)
        {
            string name = $"m_{prefix}_{Regex.Replace(key, "[^a-zA-Z]", "").ToLower()}";
            MenuItem item = FindName(name);
            if (item != null)
                return item.IsChecked;
            return true;
        }
    }
}
