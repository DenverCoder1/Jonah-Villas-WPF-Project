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
using BL;
using System.Text.RegularExpressions;
using BE;

namespace WPFPL
{
    /// <summary>
    /// Interaction logic for HostOrders.xaml
    /// </summary>
    public partial class HostOrders : Page
    {
        public MainWindow mainWindow;

        public static ObservableCollection<string> OrdersCollection { get; set; }

        public static string Search { get; set; }

        public object PascalCaseConverter { get; private set; }

        private static int SortIndex { get; set; }

        public HostOrders()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            OrdersCollection = new ObservableCollection<string>();
            Orders.ItemsSource = OrdersCollection;
            Refresh();
        }

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
                        case 0: sortedOrders = Util.Bl.GetHostOrders(Util.MyHost.HostKey).OrderBy(item => item.OrderKey).ToList(); break;
                        // Newest first
                        case 1: sortedOrders = Util.Bl.GetHostOrders(Util.MyHost.HostKey).OrderByDescending(item => item.OrderKey).ToList(); break;
                        // Hosting Unit ID increasing
                        case 2: sortedOrders = Util.Bl.GetHostOrders(Util.MyHost.HostKey).OrderBy(item => item.HostingUnitKey).ToList(); break;
                        // Hosting Unit ID decreasing
                        case 3: sortedOrders = Util.Bl.GetHostOrders(Util.MyHost.HostKey).OrderByDescending(item => item.HostingUnitKey).ToList(); break;
                        // Guest Request ID increasing
                        case 4: sortedOrders = Util.Bl.GetHostOrders(Util.MyHost.HostKey).OrderBy(item => item.GuestRequestKey).ToList(); break;
                        // Guest Request ID decreasing
                        case 5: sortedOrders = Util.Bl.GetHostOrders(Util.MyHost.HostKey).OrderByDescending(item => item.GuestRequestKey).ToList(); break;
                        // Email date first to last
                        case 6: sortedOrders = Util.Bl.GetHostOrders(Util.MyHost.HostKey).OrderBy(item => item.EmailDeliveryDate).ToList(); break;
                        // Email date last to first
                        case 7: sortedOrders = Util.Bl.GetHostOrders(Util.MyHost.HostKey).OrderByDescending(item => item.EmailDeliveryDate).ToList(); break;
                        // Order Status A-Z
                        case 8: sortedOrders = Util.Bl.GetHostOrders(Util.MyHost.HostKey).OrderBy(item => item.Status.ToString()).ToList(); break;
                        default: sortedOrders = Util.Bl.GetHostOrders(Util.MyHost.HostKey).OrderBy(item => item.OrderKey).ToList(); break;
                    }
                    // add items to list and filter by search
                    foreach (Order item in sortedOrders)
                    {
                        // search by all public fields
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

        private void Return_To_Options(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostMenu());
        }

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
                        MainWindow.Dialog($"Select a new status for Order #{orderKey}.", "HostUpdateOrder", null, oldStatus);
                    }
                }
                catch (Exception error)
                {
                    Util.GetMainWindow().MySnackbar.MessageQueue.Enqueue(error.Message);
                }
            }
        }

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

        private void Refresh_Event(object sender, RoutedEventArgs e)
        {
            Search = SearchBox.Text;
            Refresh(Search);
        }

        private void Clear_Search(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            Refresh();
        }

        private void Sort_Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            SortIndex = sortBy.SelectedIndex;
            Refresh();
        }
    }
}
