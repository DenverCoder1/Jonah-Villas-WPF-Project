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
        public static MenuItem AddMenuItem(MenuItem parent, string header, bool checkable, string parentHeader, Action<string, object> registerName, RoutedEventHandler MenuItem_Checked)
        {
            if (parent == null) { return parent; }
            // ex. TypeOfPlace => m_TypeOfPlace
            string name = Regex.Replace($"m_{parentHeader}_{header}", "[^a-zA-Z0-9_]", "");
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
            // register name
            registerName(name, menuItem);
            // add item to parent
            parent.Items.Add(menuItem);
            return menuItem;
        }

        /// <summary>
        /// Return whether an item is checked based on header
        /// </summary>
        public static bool FilterItemChecked(string header, string parentHeader, Func<string, MenuItem> findName, Action<string, object> registerName, RoutedEventHandler MenuItem_Checked)
        {
            string name = Regex.Replace($"m_{parentHeader}_{header}", "[^a-zA-Z0-9_]", "");
            MenuItem item = findName(name);
            if (item == null)
            {
                MenuItem parent = findName(Regex.Replace($"m_top_{parentHeader}", "[^a-zA-Z0-9_]", ""));
                AddMenuItem(parent, header, true, parentHeader, registerName, MenuItem_Checked);
                return true;
            }
            return item.IsChecked;
        }
    }
}
