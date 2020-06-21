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
        public MainWindow mainWindow;

        public static ObservableCollection<string> OrdersCollection { get; set; }

        public static string Search { get; set; }
        public AdminOrders()
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
                // normalize search
                if (search != null) { search = Normalize.Convert(search); }
                else { search = ""; }
                // clear collection
                OrdersCollection.Clear();
                // get items and filter by search
                foreach (BE.Order item in Util.Bl.GetOrders())
                {
                    // search by key, name, district, city, or owner id
                    if (Normalize.Convert(item).Contains(search))
                    {
                        OrdersCollection.Add(item.ToString());
                    }
                }
            }
        }

        private void Return_To_Menu(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminMenu());
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
                if (long.TryParse(match.Groups[1].Value, out long orderKey))
                {
                    ObservableCollection<string> StatusesCollection = new ObservableCollection<string>();
                    foreach(string status in Enum.GetNames(typeof(OrderStatus)))
                    {
                        StatusesCollection.Add(PascalCaseToText.Convert(status));
                    }
                    mainWindow.MyDialogComboBox1.ItemsSource = StatusesCollection;
                    string oldStatus = PascalCaseToText.Convert(Util.Bl.GetOrder(orderKey).Status.ToString());
                    MainWindow.Dialog($"Select a new status for Order #{orderKey}.", "AdminUpdateOrder", null, oldStatus);
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
    }
}
