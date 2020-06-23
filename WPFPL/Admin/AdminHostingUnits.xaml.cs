using BL;
using WPFPL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BE;

namespace WPFPL.Admin
{
    /// <summary>
    /// Interaction logic for AdminHostingUnits.xaml
    /// </summary>
    public partial class AdminHostingUnits : Page
    {
        private static readonly MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        public static ObservableCollection<string> HostingUnitCollection { get; set; }

        private static string Search { get; set; }

        private static int SortIndex { get; set; }

        public AdminHostingUnits()
        {
            InitializeComponent();
            HostingUnitCollection = new ObservableCollection<string>();
            HostingUnits.ItemsSource = HostingUnitCollection;
            Refresh();
            PopulateFilterMenu();
        }

        /// <summary>
        /// Refresh items in list and apply search and filters
        /// </summary>
        public void Refresh(object sender = null, RoutedEventArgs e = null)
        {
            if (HostingUnitCollection != null)
            {
                try
                {
                    // normalize search
                    if (Search == null) { Search = ""; }
                    string search = Normalize.Convert(Search);
                    // clear collection
                    HostingUnitCollection.Clear();
                    // list of hosting units
                    List<HostingUnit> orderedHostingUnits = new List<HostingUnit>();
                    // get hosting units and sort
                    switch (SortIndex)
                    {
                        // Oldest first
                        case -1:
                        case 0: orderedHostingUnits = MainWindow.Bl.GetHostingUnits().OrderBy(item => item.HostingUnitKey).ToList(); break;
                        // Newest first
                        case 1: orderedHostingUnits = MainWindow.Bl.GetHostingUnits().OrderByDescending(item => item.HostingUnitKey).ToList(); break;
                        // Unit name A-Z
                        case 2: orderedHostingUnits = MainWindow.Bl.GetHostingUnits().OrderBy(item => item.UnitName).ToList(); break;
                        // Unit city A-Z
                        case 3: orderedHostingUnits = MainWindow.Bl.GetHostingUnits().OrderBy(item => item.UnitCity.ToString()).ToList(); break;
                        // Unit district A-Z
                        case 4: orderedHostingUnits = MainWindow.Bl.GetHostingUnits().OrderBy(item => item.UnitDistrict.ToString()).ToList(); break;
                        // Owner ID
                        case 5: orderedHostingUnits = MainWindow.Bl.GetHostingUnits().OrderBy(item => item.Owner.HostKey.ToString()).ToList(); break;
                        // Newest first
                        default: orderedHostingUnits = MainWindow.Bl.GetHostingUnits().OrderBy(item => item.HostingUnitKey).ToList(); break;
                    }
                    MenuItem findName(string name) { return (MenuItem)FindName(name); }
                    // add items to list and filter by search
                    foreach (HostingUnit item in orderedHostingUnits)
                    {
                        // search by all public fields
                        if (Normalize.Convert(item).Contains(search))
                        {
                            // apply advanced filters
                            if (FilterMenus.FilterItemChecked(item.UnitName.ToString(), "name", findName) &&
                                FilterMenus.FilterItemChecked(item.UnitDistrict.ToString(), "district", findName) &&
                                FilterMenus.FilterItemChecked(item.UnitCity.ToString(), "city", findName) &&
                                FilterMenus.FilterItemChecked(item.Owner.HostKey.ToString(), "hostkey", findName))
                            {
                                HostingUnitCollection.Add(item.ToString());
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    mainWindow.MySnackbar.MessageQueue.Enqueue(error.Message);
                }
            }
        }

        /// <summary>
        /// Populate the Advanced Filter menu
        /// </summary>
        private void PopulateFilterMenu()
        {
            // Create sub-menus
            void registerName(string name, object scopedElement) { RegisterName(name, scopedElement); }
            MenuItem unitName = FilterMenus.AddMenuItem(FilterMenu, "Hosting unit name", false, "top", registerName, Refresh);
            MenuItem district = FilterMenus.AddMenuItem(FilterMenu, "District", false, "top", registerName, Refresh);
            MenuItem city = FilterMenus.AddMenuItem(FilterMenu, "City", false, "top", registerName, Refresh);
            MenuItem hostkey = FilterMenus.AddMenuItem(FilterMenu, "Owner ID", false, "top", registerName, Refresh);

            var matches = MainWindow.Bl.GetHostingUnits();

            // Add unit name items
            foreach (string item in (from item in matches
                                     orderby item.UnitName
                                     select item.UnitName).Distinct().ToList())
                FilterMenus.AddMenuItem(unitName, item, true, "name", registerName, Refresh);

            // Add district items
            foreach (string item in (from item in matches
                                     orderby item.UnitDistrict.ToString()
                                     select item.UnitDistrict.ToString()).Distinct().ToList())
                FilterMenus.AddMenuItem(district, item, true, "district", registerName, Refresh);

            // Add city items
            foreach (string item in (from item in matches
                                     orderby item.UnitCity.ToString()
                                     select item.UnitCity.ToString()).Distinct().ToList())
                FilterMenus.AddMenuItem(city, item, true, "city", registerName, Refresh);

            // Add host key items
            foreach (string item in (from item in matches
                                     orderby item.Owner.HostKey.ToString()
                                     select item.Owner.HostKey.ToString()).Distinct().ToList())
                FilterMenus.AddMenuItem(hostkey, item, true, "hostkey", registerName, Refresh);
        }

        /// <summary>
        /// Button to return to admin menu
        /// </summary>
        private void Return_To_Menu(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminMenu());
        }

        /// <summary>
        /// Button to force refresh of list
        /// </summary>
        private void Refresh_Event(object sender, RoutedEventArgs e)
        {
            Search = SearchBox.Text;
            Refresh(Search);
        }

        /// <summary>
        /// Empty search text box
        /// </summary>
        private void Clear_Search(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            Refresh();
        }

        /// <summary>
        /// On change sort method in comboBox,
        /// refresh the list
        /// </summary>
        private void Sort_Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            SortIndex = sortBy.SelectedIndex;
            Refresh();
        }
    }
}
