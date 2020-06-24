using BE;
using BL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private static readonly MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        public static ObservableCollection<string> HostCollection { get; set; }

        private static string Search { get; set; }

        private static int SortIndex { get; set; }

        public AdminHosts()
        {
            InitializeComponent();
            HostCollection = new ObservableCollection<string>();
            Hosts.ItemsSource = HostCollection;
            Refresh();
        }

        /// <summary>
        /// Refresh items in list and apply search and filters
        /// </summary>
        public void Refresh(object sender = null, RoutedEventArgs e = null)
        {
            if (HostCollection != null)
            {
                try
                {
                    // normalize search
                    if (Search == null) { Search = ""; }
                    string search = Normalize.Convert(Search);
                    // clear collection
                    HostCollection.Clear();
                    // list of orders
                    List<Host> sortedHosts = new List<Host>();
                    // get orders and sort
                    switch (SortIndex)
                    {
                        case -1:
                        // Oldest first
                        case 0: sortedHosts = MainWindow.Bl.GetHosts().OrderBy(item => item.HostKey).ToList(); break;
                        // Newest first
                        case 1: sortedHosts = MainWindow.Bl.GetHosts().OrderByDescending(item => item.HostKey).ToList(); break;
                        // Last name A-Z
                        case 2: sortedHosts = MainWindow.Bl.GetHosts().OrderBy(item => item.LastName).ToList(); break;
                        // First name A-Z
                        case 3: sortedHosts = MainWindow.Bl.GetHosts().OrderBy(item => item.FirstName).ToList(); break;
                        default: sortedHosts = MainWindow.Bl.GetHosts().OrderBy(item => item.HostKey).ToList(); break;
                    }
                    // define delegate actions for finding and creating filter items
                    MenuItem findName(string name) { return (MenuItem)FindName(name); }
                    void registerName(string name, object scopedElement) { if (FindName(name) == null) RegisterName(name, scopedElement); }
                    // add items to list and filter by search
                    foreach (Host item in sortedHosts)
                    {
                        // search by all public fields
                        if (Normalize.Convert(item).Contains(search))
                        {
                            // apply advanced filters
                            if (FilterMenus.FilterItemChecked(item.BankClearance.ToString(), "Bank Clearance", findName, registerName, Refresh))
                            {
                                HostCollection.Add(item.ToString());
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

        /// <summary>
        /// Prompt for deleting hosting unit
        /// </summary>
        private void UpdateBankClearance(object sender, RoutedEventArgs e)
        {
            if (Hosts.SelectedItem == null)
            {
                MainWindow.Dialog("First select a host to update.");
                return;
            }
            Match match = new Regex(@"^#(\d+) .*").Match(Hosts.SelectedItem.ToString());
            if (match.Success)
            {
                if (long.TryParse(match.Groups[1].Value, out long huKey))
                {
                    var host = MainWindow.Bl.GetHost(huKey);
                    var BankClearanceOptions = new ObservableCollection<string> { "True", "False" };
                    mainWindow.MyDialogComboBox1.ItemsSource = BankClearanceOptions;
                    MainWindow.Dialog($"Select the bank clearance status for Host #{huKey}.", "AdminUpdateBankClearance", null, host.BankClearance.ToString(), null, null);
                }
            }
        }

        /// <summary>
        /// finish deletion when prompt closed
        /// </summary>
        /// <param name="dialogText">Text from the dialog prompt</param>
        /// <param name="status">New bank clearance status</param>
        public void FinishUpdateBankClearance(string dialogText, string status)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            if (string.IsNullOrEmpty(status))
            {
                mainWindow.MySnackbar.MessageQueue.Enqueue("Action was cancelled.");
                return;
            }

            Match match = new Regex(@".*Host #(\d+).*").Match(dialogText);
            if (match.Success)
            {
                if (long.TryParse(match.Groups[1].Value, out long hostKey))
                {
                    try
                    {
                        Host host = MainWindow.Bl.GetHost(hostKey);

                        if (host.BankClearance.ToString() != status)
                        {
                            host.BankClearance = (status == "True");

                            if (MainWindow.Bl.UpdateHost(host))
                                mainWindow.MySnackbar.MessageQueue.Enqueue("Host was successfully updated.");
                        }
                        else
                        {
                            throw new Exception("Host was not changed.");
                        }
                    }
                    catch (Exception error)
                    {
                        mainWindow.MySnackbar.MessageQueue.Enqueue(error.Message.ToString());
                    }

                    Refresh();
                }
                else
                {
                    mainWindow.MySnackbar.MessageQueue.Enqueue("Action was cancelled.");
                }
            }
        }
    }
}
