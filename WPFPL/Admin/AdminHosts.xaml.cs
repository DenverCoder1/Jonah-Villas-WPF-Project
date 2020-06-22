using BE;
using BL;
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

namespace WPFPL.Admin
{
    /// <summary>
    /// Interaction logic for AdminHosts.xaml
    /// </summary>
    public partial class AdminHosts : Page
    {
        private readonly MainWindow mainWindow;

        public static ObservableCollection<string> HostCollection { get; set; }

        private static string Search { get; set; }

        private static int SortIndex { get; set; }

        public AdminHosts()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            HostCollection = new ObservableCollection<string>();
            Hosts.ItemsSource = HostCollection;
            Refresh();
        }

        /// <summary>
        /// Refresh items in list and apply search and filters
        /// </summary>
        /// <param name="search">search to filter on</param>
        public static void Refresh(string search = "")
        {
            if (HostCollection != null)
            {
                try
                {
                    // normalize search
                    if (search != null) { search = Normalize.Convert(search); }
                    else { search = ""; }
                    // clear collection
                    HostCollection.Clear();
                    // list of orders
                    List<Host> sortedHosts = new List<Host>();
                    // get orders and sort
                    switch (SortIndex)
                    {
                        case -1:
                        // Oldest first
                        case 0: sortedHosts = Util.Bl.GetHosts().OrderBy(item => item.HostKey).ToList(); break;
                        // Newest first
                        case 1: sortedHosts = Util.Bl.GetHosts().OrderByDescending(item => item.HostKey).ToList(); break;
                        // Last name A-Z
                        case 2: sortedHosts = Util.Bl.GetHosts().OrderBy(item => item.LastName).ToList(); break;
                        // First name A-Z
                        case 3: sortedHosts = Util.Bl.GetHosts().OrderBy(item => item.FirstName).ToList(); break;
                        default: sortedHosts = Util.Bl.GetHosts().OrderBy(item => item.HostKey).ToList(); break;
                    }
                    // add items to list and filter by search
                    foreach (Host item in sortedHosts)
                    {
                        // search by all public fields
                        if (Normalize.Convert(item).Contains(search))
                        {
                            HostCollection.Add(item.ToString());
                        }
                    }
                }
                catch (Exception error)
                {
                    Util.GetMainWindow().MySnackbar.MessageQueue.Enqueue(error.Message);
                }
            }
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
