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
        private readonly MainWindow mainWindow;

        public static ObservableCollection<string> HostingUnitCollection { get; set; }

        private static string Search { get; set; }

        private static int SortIndex { get; set; }

        public AdminHostingUnits()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            HostingUnitCollection = new ObservableCollection<string>();
            HostingUnits.ItemsSource = HostingUnitCollection;
            Refresh();
        }

        /// <summary>
        /// Refresh items in list and apply search and filters
        /// </summary>
        /// <param name="search">search to filter on</param>
        public static void Refresh(string search = "")
        {
            if (HostingUnitCollection != null)
            {
                try
                {
                    // normalize search
                    if (search != null) { search = Normalize.Convert(search); }
                    else { search = ""; }
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
                        // Newest first
                        default: orderedHostingUnits = MainWindow.Bl.GetHostingUnits().OrderBy(item => item.HostingUnitKey).ToList(); break;
                    }
                    // add items to list and filter by search
                    foreach (HostingUnit item in orderedHostingUnits)
                    {
                        // search by all public fields
                        if (Normalize.Convert(item).Contains(search))
                        {
                            HostingUnitCollection.Add(item.ToString());
                        }
                    }
                }
                catch (Exception error)
                {
                    ((MainWindow)Application.Current.MainWindow).MySnackbar.MessageQueue.Enqueue(error.Message);
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
