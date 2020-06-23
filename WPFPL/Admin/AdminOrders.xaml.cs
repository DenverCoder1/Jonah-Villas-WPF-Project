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
using System.Text.RegularExpressions;
using BE;

namespace WPFPL.Admin
{
    /// <summary>
    /// Interaction logic for AdminOrders.xaml
    /// </summary>
    public partial class AdminOrders : Page
    {
        private static readonly MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        public static ObservableCollection<string> OrdersCollection { get; set; }

        private static string Search { get; set; }

        private static int SortIndex { get; set; }

        public AdminOrders()
        {
            InitializeComponent();
            OrdersCollection = new ObservableCollection<string>();
            Orders.ItemsSource = OrdersCollection;
            Refresh();
            PopulateFilterMenu();
        }

        /// <summary>
        /// Refresh items in list and apply search and filters
        /// </summary>
        public void Refresh(object sender = null, RoutedEventArgs e = null)
        {
            if (OrdersCollection != null)
            {
                try
                {
                    // normalize search
                    if (Search == null) { Search = ""; }
                    string search = Normalize.Convert(Search);
                    // clear collection
                    OrdersCollection.Clear();
                    // list of orders
                    List<Order> sortedOrders = new List<Order>();
                    // get orders and sort
                    switch (SortIndex)
                    {
                        case -1:
                        // Oldest first
                        case 0: sortedOrders = MainWindow.Bl.GetOrders().OrderBy(item => item.OrderKey).ToList(); break;
                        // Newest first
                        case 1: sortedOrders = MainWindow.Bl.GetOrders().OrderByDescending(item => item.OrderKey).ToList(); break;
                        // Hosting Unit ID increasing
                        case 2: sortedOrders = MainWindow.Bl.GetOrders().OrderBy(item => item.HostingUnitKey).ToList(); break;
                        // Hosting Unit ID decreasing
                        case 3: sortedOrders = MainWindow.Bl.GetOrders().OrderByDescending(item => item.HostingUnitKey).ToList(); break;
                        // Guest Request ID increasing
                        case 4: sortedOrders = MainWindow.Bl.GetOrders().OrderBy(item => item.GuestRequestKey).ToList(); break;
                        // Guest Request ID decreasing
                        case 5: sortedOrders = MainWindow.Bl.GetOrders().OrderByDescending(item => item.GuestRequestKey).ToList(); break;
                        // Email date first to last
                        case 6: sortedOrders = MainWindow.Bl.GetOrders().OrderBy(item => item.EmailDeliveryDate).ToList(); break;
                        // Email date last to first
                        case 7: sortedOrders = MainWindow.Bl.GetOrders().OrderByDescending(item => item.EmailDeliveryDate).ToList(); break;
                        // Order Status A-Z
                        case 8: sortedOrders = MainWindow.Bl.GetOrders().OrderBy(item => item.Status.ToString()).ToList(); break;
                        default: sortedOrders = MainWindow.Bl.GetOrders().OrderBy(item => item.OrderKey).ToList(); break;
                    }
                    MenuItem findName(string name) { return (MenuItem)FindName(name); }
                    // add items to list and filter by search
                    foreach (Order item in sortedOrders)
                    {
                        // search by key, name, district, city, or owner id
                        if (Normalize.Convert(item).Contains(search))
                        {
                            // apply advanced filters
                            if (FilterMenus.FilterItemChecked(item.Status.ToString(), "status", findName) &&
                                FilterMenus.FilterItemChecked(item.HostingUnitKey.ToString(), "unit", findName) &&
                                FilterMenus.FilterItemChecked(item.GuestRequestKey.ToString(), "request", findName))
                            {
                                OrdersCollection.Add(item.ToString());
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
            MenuItem status = FilterMenus.AddMenuItem(FilterMenu, "Status", false, "top", registerName, Refresh);
            MenuItem unit = FilterMenus.AddMenuItem(FilterMenu, "Hosting Unit ID", false, "top", registerName, Refresh);
            MenuItem request = FilterMenus.AddMenuItem(FilterMenu, "Guest Request ID", false, "top", registerName, Refresh);

            var matches = MainWindow.Bl.GetOrders();

            // Add status items
            foreach (string item in (from item in matches
                                     orderby item.Status.ToString()
                                     select item.Status.ToString()).Distinct().ToList())
                FilterMenus.AddMenuItem(status, item, true, "status", registerName, Refresh);

            // Add hosting unit id items
            foreach (string item in (from item in matches
                                     orderby item.HostingUnitKey.ToString()
                                     select item.HostingUnitKey.ToString()).Distinct().ToList())
                FilterMenus.AddMenuItem(unit, item, true, "unit", registerName, Refresh);

            // Add guest request id items
            foreach (string item in (from item in matches
                                     orderby item.GuestRequestKey.ToString()
                                     select item.GuestRequestKey.ToString()).Distinct().ToList())
                FilterMenus.AddMenuItem(request, item, true, "request", registerName, Refresh);
        }

        /// <summary>
        /// Button to return to admin menu
        /// </summary>
        private void Return_To_Menu(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminMenu());
        }

        /// <summary>
        /// Prompt for updating the status of an order
        /// </summary>
        private void Update_Order(object sender, RoutedEventArgs e)
        {
            if (Orders.SelectedItem == null)
            {
                MainWindow.Dialog("Select an order to update.");
                return;
            }
            Match match = new Regex(@"^#(\d+) .*").Match(Orders.SelectedItem.ToString());
            if (match.Success)
            {
                try
                {
                    if (long.TryParse(match.Groups[1].Value, out long orderKey))
                    {
                        ObservableCollection<string> StatusesCollection = new ObservableCollection<string>();
                        foreach (string status in Enum.GetNames(typeof(OrderStatus)))
                        {
                            StatusesCollection.Add(PascalCaseToText.Convert(status));
                        }
                        mainWindow.MyDialogComboBox1.ItemsSource = StatusesCollection;
                        string oldStatus = PascalCaseToText.Convert(MainWindow.Bl.GetOrder(orderKey).Status.ToString());
                        MainWindow.Dialog($"Select a new status for Order #{orderKey}.", "AdminUpdateOrder", null, oldStatus);
                    }
                }
                catch (Exception error)
                {
                    mainWindow.MySnackbar.MessageQueue.Enqueue(error.Message);
                }
            }
        }

        /// <summary>
        /// Finish update of order when prompt closed
        /// </summary>
        /// <param name="dialogText">Text placed in the dialog box prompt</param>
        /// <param name="selection">Selected status in dialog</param>
        public void Finish_Update_Order(string dialogText, object selection)
        {
            if (selection != null)
            {
                Match orderKeyMatch = new Regex(@".* #(\d+)\..*").Match(dialogText);
                if (orderKeyMatch.Success)
                {
                    if (long.TryParse(orderKeyMatch.Groups[1].Value, out long orderKey))
                    {
                        try
                        {
                            if (Enum.TryParse(selection.ToString().Replace(" ", ""), out OrderStatus status))
                            {
                                Order order = MainWindow.Bl.GetOrder(orderKey);
                                if (order.Status == status)
                                    throw new Exception("Status was not changed.");
                                order.Status = status;
                                if (MainWindow.Bl.UpdateOrder(order))
                                    mainWindow.MySnackbar.MessageQueue.Enqueue("Order was successfully updated.");
                                Refresh();
                                return;
                            }
                        }
                        catch (Exception error)
                        {
                            mainWindow.MySnackbar.MessageQueue.Enqueue(error.Message);
                            return;
                        }
                    }
                }
            }

            mainWindow.MySnackbar.MessageQueue.Enqueue("Action was cancelled.");
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
