using BE;
using BL;
using WPFPL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace WPFPL.Admin
{
    /// <summary>
    /// Interaction logic for AdminRequests.xaml
    /// </summary>
    public partial class AdminRequests : Page
    {
        private static readonly MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        public static ObservableCollection<string> RequestCollection { get; set; }

        private static string Search { get; set; }

        private static int SortIndex { get; set; }

        public AdminRequests()
        {
            InitializeComponent();
            RequestCollection = new ObservableCollection<string>();
            Requests.ItemsSource = RequestCollection;
            Refresh();
            PopulateFilterMenu();
        }

        /// <summary>
        /// Refresh items in list and apply search and filters
        /// </summary>
        public void Refresh(object sender = null, RoutedEventArgs e = null)
        {
            if (RequestCollection != null)
            {
                try
                {
                    // normalize search
                    if (Search == null) { Search = ""; }
                    string search = Normalize.Convert(Search);
                    // clear collection
                    RequestCollection.Clear();
                    // list of requests
                    List<GuestRequest> orderedRequests = new List<GuestRequest>();
                    // get requests and sort
                    switch (SortIndex)
                    {
                        case -1:
                        // Oldest first
                        case 0: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderBy(item => item.GuestRequestKey).ToList(); break;
                        // Newest first
                        case 1: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderByDescending(item => item.GuestRequestKey).ToList(); break;
                        // Last name A-Z
                        case 2: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderBy(item => item.LastName).ToList(); break;
                        // First name A-Z
                        case 3: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderBy(item => item.FirstName).ToList(); break;
                        // Fewest guests first
                        case 4: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderBy(item => item.NumAdults + item.NumChildren).ToList(); break;
                        // Most guests first
                        case 5: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderByDescending(item => item.NumAdults + item.NumChildren).ToList(); break;
                        // Unit Type A-Z
                        case 6: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderBy(item => item.PrefType.ToString()).ToList(); break;
                        // Unit City A-Z
                        case 7: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderBy(item => item.PrefCity.ToString()).ToList(); break;
                        // Unit District A-Z
                        case 8: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderBy(item => item.PrefDistrict.ToString()).ToList(); break;
                        // Entry date soonest first
                        case 9: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderBy(item => item.EntryDate).ToList(); break;
                        // Entry date furthest first
                        case 10: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderByDescending(item => item.EntryDate).ToList(); break;
                        // Request Status A-Z
                        case 11: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderBy(item => item.Status.ToString()).ToList(); break;
                        default: orderedRequests = MainWindow.Bl.GetGuestRequests().OrderBy(item => item.GuestRequestKey).ToList(); break;
                    }
                    MenuItem findName(string name) { return (MenuItem)FindName(name); }
                    // add items to list and filter by search
                    foreach (GuestRequest item in orderedRequests)
                    {
                        // search by all public fields
                        if (Normalize.Convert(item).Contains(search))
                        {
                            // apply advanced filters
                            if (FilterMenus.FilterItemChecked(item.Status.ToString(), "status", findName) &&
                                FilterMenus.FilterItemChecked(item.PrefType.ToString(), "type", findName) &&
                                FilterMenus.FilterItemChecked(item.PrefDistrict.ToString(), "district", findName) &&
                                FilterMenus.FilterItemChecked(item.PrefCity.ToString(), "city", findName))
                            {
                                RequestCollection.Add(item.ToString());
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
        /// Populate the Advanced Filter menu with enums
        /// </summary>
        private void PopulateFilterMenu()
        {
            // Create sub-menus
            void registerName(string name, object scopedElement) { RegisterName(name, scopedElement); }
            MenuItem status = FilterMenus.AddMenuItem(FilterMenu, "Status", false, "top", registerName, Refresh);
            MenuItem type = FilterMenus.AddMenuItem(FilterMenu, "Type of Place", false, "top", registerName, Refresh);
            MenuItem district = FilterMenus.AddMenuItem(FilterMenu, "District", false, "top", registerName, Refresh);
            MenuItem city = FilterMenus.AddMenuItem(FilterMenu, "City", false, "top", registerName, Refresh);

            var matches = MainWindow.Bl.GetGuestRequests();

            // Add status items
            foreach (string item in (from item in matches
                                     orderby item.Status.ToString()
                                     select item.Status.ToString()).Distinct().ToList())
                FilterMenus.AddMenuItem(status, item, true, "status", registerName, Refresh);

            // Add type items
            foreach (string item in (from item in matches
                                     orderby item.PrefType.ToString()
                                     select item.PrefType.ToString()).Distinct().ToList())
                FilterMenus.AddMenuItem(type, item, true, "type", registerName, Refresh);

            // Add district items
            foreach (string item in (from item in matches
                                     orderby item.PrefDistrict.ToString()
                                     select item.PrefDistrict.ToString()).Distinct().ToList())
                FilterMenus.AddMenuItem(district, item, true, "district", registerName, Refresh);

            // Add city items
            foreach (string item in (from item in matches
                                     orderby item.PrefCity.ToString()
                                     select item.PrefCity.ToString()).Distinct().ToList())
                FilterMenus.AddMenuItem(city, item, true, "city", registerName, Refresh);
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
