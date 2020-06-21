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
using BE;
using System.Text.RegularExpressions;
using System.IO;
using System.ComponentModel;

namespace WPFPL
{
    /// <summary>
    /// Interaction logic for HostRequests.xaml
    /// </summary>
    public partial class HostRequests : Page
    {
        public MainWindow mainWindow;

        public static ObservableCollection<string> RequestCollection { get; set; }

        public static ObservableCollection<string> HostingUnitCollection { get; set; }

        public static string Search { get; set; }

        private static int SortIndex { get; set; }

        public HostRequests()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            RequestCollection = new ObservableCollection<string>();
            HostingUnitCollection = new ObservableCollection<string>();
            Requests.ItemsSource = RequestCollection;
            Refresh();
        }

        public static void Refresh(string search = "")
        {
            if (RequestCollection != null)
            {
                try
                {
                    // normalize search
                    if (search != null) { search = Normalize.Convert(search); }
                    else { search = ""; }
                    // clear collection
                    RequestCollection.Clear();
                    // list of requests
                    List<GuestRequest> orderedRequests = new List<GuestRequest>();
                    // get requests and sort
                    switch (SortIndex)
                    {
                        case -1:
                        // Oldest first
                        case 0: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderBy(item => item.GuestRequestKey).ToList(); break;
                        // Newest first
                        case 1: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderByDescending(item => item.GuestRequestKey).ToList(); break;
                        // Last name A-Z
                        case 2: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderBy(item => item.LastName).ToList(); break;
                        // First name A-Z
                        case 3: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderBy(item => item.FirstName).ToList(); break;
                        // Fewest guests first
                        case 4: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderBy(item => item.NumAdults + item.NumChildren).ToList(); break;
                        // Most guests first
                        case 5: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderByDescending(item => item.NumAdults + item.NumChildren).ToList(); break;
                        // Unit Type A-Z
                        case 6: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderBy(item => item.PrefType.ToString()).ToList(); break;
                        // Unit City A-Z
                        case 7: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderBy(item => item.PrefCity.ToString()).ToList(); break;
                        // Unit District A-Z
                        case 8: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderBy(item => item.PrefDistrict.ToString()).ToList(); break;
                        // Entry date soonest first
                        case 9: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderBy(item => item.EntryDate).ToList(); break;
                        // Entry date furthest first
                        case 10: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderByDescending(item => item.EntryDate).ToList(); break;
                        // Request Status A-Z
                        case 11: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderBy(item => item.Status.ToString()).ToList(); break;
                        default: orderedRequests = Util.Bl.GetOpenGuestRequests().OrderBy(item => item.GuestRequestKey).ToList(); break;
                    }
                    // add items to list and filter by search
                    foreach (GuestRequest item in orderedRequests)
                    {
                        // search by all public fields
                        if (Normalize.Convert(item).Contains(search))
                        {
                            RequestCollection.Add(item.ToString());
                        }
                    }
                }
                catch (Exception error)
                {
                    Util.GetMainWindow().MySnackbar.MessageQueue.Enqueue(error.Message);
                }
            }
        }

        private void UpdateAvailableHostingUnits(long grKey)
        {
            try
            {
                if (HostingUnitCollection != null)
                {
                    HostingUnitCollection.Clear();
                    foreach (HostingUnit item in Util.Bl.GetAvailableHostHostingUnits(Util.MyHost.HostKey, grKey))
                    {
                        HostingUnitCollection.Add(item.ToString());
                    }
                    if (HostingUnitCollection.Count == 0)
                        HostingUnitCollection.Add("No available units");
                }
            }
            catch (Exception error)
            {
                mainWindow.MySnackbar.MessageQueue.Enqueue(error.Message);
            }
        }

        private void Return_To_Options(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostMenu());
        }

        private void Create_Order(object sender, RoutedEventArgs e)
        {
            if (Requests.SelectedItem == null)
            {
                MainWindow.Dialog("Select a customer request before creating an order.");
                return;
            }
            Match match = new Regex(@"^#(\d+) .*").Match(Requests.SelectedItem.ToString());
            if (match.Success)
            {
                if (long.TryParse(match.Groups[1].Value, out long grKey))
                {
                    UpdateAvailableHostingUnits(grKey);
                    mainWindow.MyDialogComboBox1.ItemsSource = HostingUnitCollection;
                    MainWindow.Dialog($"Which Hosting Unit do you you want to add Request #{grKey} to?", "HostCreateOrder", null, "");
                }
            }
        }

        public static void Finish_Create_Order(string dialogText, object selection)
        {
            if (selection != null)
            {
                Match grKeyMatch = new Regex(@".*Request #(\d+).*").Match(dialogText);
                if (grKeyMatch.Success)
                {
                    if (long.TryParse(grKeyMatch.Groups[1].Value, out long grKey))
                    {
                        try
                        {
                            Match huKeyMatch = new Regex(@"^#(\d+) :.*").Match(selection.ToString());
                            if (huKeyMatch.Success)
                            {
                                if (long.TryParse(huKeyMatch.Groups[1].Value, out long huKey))
                                {
                                    Order order = new Order(huKey, grKey);
                                    Util.Bl.CreateOrder(order, EmailWorkerCompleted);
                                    Util.GetMainWindow().MySnackbar.MessageQueue.Enqueue("Success! An email will be sent to the customer.");
                                    Refresh();
                                    return;
                                }
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
        /// Get results from email attempt, show error if applicable
        /// </summary>
        public static void EmailWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            object result = e.Result;
            if (result is List<object> resList)
            {
                // get results from list
                Order order = (Order) resList[0];
                object outcome = resList[1];

                if (outcome is Exception error)
                {
                    Util.GetMainWindow().MySnackbar.MessageQueue.Enqueue(error.Message+$" Retrying in 3 seconds.");
                    // wait a few seconds and retry sending email (delay happens in the background worker)
                    Mailing.StartEmailBackgroundWorker(order, EmailWorkerCompleted, delay: 3000);
                    return;
                }
                else if (outcome is bool b && b == true)
                {
                    // Success!
                    Util.GetMainWindow().MySnackbar.MessageQueue.Enqueue("Email was sent successfully.");
                    return;
                }
            }
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
