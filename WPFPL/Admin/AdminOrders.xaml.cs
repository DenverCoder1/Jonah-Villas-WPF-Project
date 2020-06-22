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
        private readonly MainWindow mainWindow;

        public static ObservableCollection<string> OrdersCollection { get; set; }

        private static string Search { get; set; }

        private static int SortIndex { get; set; }

        public AdminOrders()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            OrdersCollection = new ObservableCollection<string>();
            Orders.ItemsSource = OrdersCollection;
            Refresh();
        }

        /// <summary>
        /// Refresh items in list and apply search and filters
        /// </summary>
        /// <param name="search">search to filter on</param>
        public static void Refresh(string search = "")
        {
            if (OrdersCollection != null)
            {
                try
                {
                    // normalize search
                    if (search != null) { search = Normalize.Convert(search); }
                    else { search = ""; }
                    // clear collection
                    OrdersCollection.Clear();
                    // list of orders
                    List<Order> sortedOrders = new List<Order>();
                    // get orders and sort
                    switch (SortIndex)
                    {
                        case -1:
                        // Oldest first
                        case 0: sortedOrders = Util.Bl.GetOrders().OrderBy(item => item.OrderKey).ToList(); break;
                        // Newest first
                        case 1: sortedOrders = Util.Bl.GetOrders().OrderByDescending(item => item.OrderKey).ToList(); break;
                        // Hosting Unit ID increasing
                        case 2: sortedOrders = Util.Bl.GetOrders().OrderBy(item => item.HostingUnitKey).ToList(); break;
                        // Hosting Unit ID decreasing
                        case 3: sortedOrders = Util.Bl.GetOrders().OrderByDescending(item => item.HostingUnitKey).ToList(); break;
                        // Guest Request ID increasing
                        case 4: sortedOrders = Util.Bl.GetOrders().OrderBy(item => item.GuestRequestKey).ToList(); break;
                        // Guest Request ID decreasing
                        case 5: sortedOrders = Util.Bl.GetOrders().OrderByDescending(item => item.GuestRequestKey).ToList(); break;
                        // Email date first to last
                        case 6: sortedOrders = Util.Bl.GetOrders().OrderBy(item => item.EmailDeliveryDate).ToList(); break;
                        // Email date last to first
                        case 7: sortedOrders = Util.Bl.GetOrders().OrderByDescending(item => item.EmailDeliveryDate).ToList(); break;
                        // Order Status A-Z
                        case 8: sortedOrders = Util.Bl.GetOrders().OrderBy(item => item.Status.ToString()).ToList(); break;
                        default: sortedOrders = Util.Bl.GetOrders().OrderBy(item => item.OrderKey).ToList(); break;
                    }
                    // add items to list and filter by search
                    foreach (Order item in sortedOrders)
                    {
                        // search by key, name, district, city, or owner id
                        if (Normalize.Convert(item).Contains(search))
                        {
                            OrdersCollection.Add(item.ToString());
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
                        string oldStatus = PascalCaseToText.Convert(Util.Bl.GetOrder(orderKey).Status.ToString());
                        MainWindow.Dialog($"Select a new status for Order #{orderKey}.", "AdminUpdateOrder", null, oldStatus);
                    }
                }
                catch (Exception error)
                {
                    Util.GetMainWindow().MySnackbar.MessageQueue.Enqueue(error.Message);
                }
            }
        }

        /// <summary>
        /// Finish update of order when prompt closed
        /// </summary>
        /// <param name="dialogText">Text placed in the dialog box prompt</param>
        /// <param name="selection">Selected status in dialog</param>
        public static void Finish_Update_Order(string dialogText, object selection)
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
                                Order order = Util.Bl.GetOrder(orderKey);
                                if (order.Status == status)
                                    throw new Exception("Status was not changed.");
                                order.Status = status;
                                if (Util.Bl.UpdateOrder(order))
                                    Util.GetMainWindow().MySnackbar.MessageQueue.Enqueue("Order was successfully updated.");
                                Refresh();
                                return;
                            }
                        }
                        catch (Exception error)
                        {
                            Util.GetMainWindow().MySnackbar.MessageQueue.Enqueue(error.Message);
                            return;
                        }
                    }
                }
            }

            Util.GetMainWindow().MySnackbar.MessageQueue.Enqueue("Action was cancelled.");
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
