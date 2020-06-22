using WPFPL;
using System;
using System.Collections.Generic;
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
using System.IO;
using BE;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace WPFPL
{
    /// <summary>
    /// Interaction logic for HostSignUp.xaml
    /// </summary>
    public partial class HostSignUp : Page
    {
        private readonly MainWindow mainWindow;

        private static XDocument BankBranchXML { get; set; }

        private static ObservableCollection<string> BankCollection { get; set; }

        private static ObservableCollection<string> BankCityCollection { get; set; }

        private static ObservableCollection<string> BankBranchCollection { get; set; }

        private static List<BankBranch> BankBranches { get; set; }

        public HostSignUp()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BankCollection = new ObservableCollection<string>();
            BankCityCollection = new ObservableCollection<string>();
            BankBranchCollection = new ObservableCollection<string>();
            hBank.ItemsSource = BankCollection;
            hBankCity.ItemsSource = BankCityCollection;
            hBankBranch.ItemsSource = BankBranchCollection;
            ListBanks();
        }

        /// <summary>
        /// Get a list of bank branches (using background worker)
        /// </summary>
        public static void ListBanks()
        {
            if (BankCollection != null)
            {
                try
                {
                    Util.Bl.GetBankBranches(GetBranchesCompleted);
                } catch (Exception error)
                {
                    Util.GetMainWindow().MySnackbar.MessageQueue.Enqueue(error.Message);
                }
            }
        }

        /// <summary>
        /// Get results from bank branch fetch
        /// </summary>
        private static void GetBranchesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            object result = e.Result;
            if (result is string xml)
            {
                BankBranchXML = XDocument.Parse(xml);

                List<string> banks = (from item in BankBranchXML.Descendants("BRANCH")
                                      select (string)item.Element("Bank_Name").Value.Trim()).Distinct().ToList();

                banks.Sort();

                BankCollection.Clear();
                foreach (string item in banks)
                {
                    BankCollection.Add(item.ToString());
                }
            }
        }
        
        private void Bank_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (hBank.SelectedItem == null) return;

            // Get a list of cities for selected Bank
            List<string> cities = (from item in BankBranchXML.Descendants("BRANCH")
                                   let bank = hBank.SelectedItem.ToString()
                                   where (string)item.Element("Bank_Name") == bank 
                                        && item.Element("City").Value != ""
                                   select (item.Element("City").Value.Trim())).Distinct().ToList();

            cities.Sort();

            BankCityCollection.Clear();
            foreach (string item in cities)
            {
                BankCityCollection.Add(item.ToString());
            }
        }

        private void BankCity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (hBank.SelectedItem == null || hBankCity.SelectedItem == null) return;

            // Get a list of branches for selected Bank/City
            List<string> branches = (from item in BankBranchXML.Descendants("BRANCH")
                                     let bank = hBank.SelectedItem.ToString()
                                     let city = hBankCity.SelectedItem.ToString()
                                     where (string)item.Element("Bank_Name") == bank
                                            && (string)item.Element("City") == city
                                            && item.Element("Address").Value != ""
                                     select $"#{item.Element("Branch_Code").Value} - {item.Element("Address").Value}").ToList();

            BankBranchCollection.Clear();
            foreach (string item in branches)
            {
                BankBranchCollection.Add(item.ToString());
            }

            // store all info on branches in list
            BankBranches = (from item in BankBranchXML.Descendants("BRANCH")
                            let bank = hBank.SelectedItem.ToString()
                            let city = hBankCity.SelectedItem.ToString()
                            where (string)item.Element("Bank_Name") == bank
                                && (string)item.Element("City") == city
                            select new BankBranch
                            {
                                BankCode = item.Element("Bank_Code").Value,
                                BankName = item.Element("Bank_Name").Value,
                                BranchCode = item.Element("Branch_Code").Value,
                                BranchAddress = item.Element("Address").Value,
                                BranchCity = item.Element("City").Value
                            }).ToList();
        }

        private void Create_Account(object sender, RoutedEventArgs e)
        {
            string fname = hFirstName.Text;
            string lname = hLastName.Text;
            string email = hEmail.Text;
            string phone = hPhone.Text;
            int branchIndex = hBankBranch.SelectedIndex;
            BankBranch bankBranch = new BankBranch
            {
                BankCode = BankBranches[branchIndex].BankCode,
                BankName = BankBranches[branchIndex].BankName,
                BranchCode = BankBranches[branchIndex].BranchCode,
                BranchAddress = BankBranches[branchIndex].BranchAddress,
                BranchCity = BankBranches[branchIndex].BranchCity
            };
            string routing = hRoutingNumber.Text;

            try
            {
                Util.Bl.ValidateHostSignUp(fname, lname, email, phone, bankBranch, routing);
            }
            catch (InvalidDataException error)
            {
                MainWindow.Dialog(error.Message.ToString());
                return;
            }

            if (!long.TryParse(Regex.Replace(phone, "[^0-9]", ""), out long phoneNum))
                return;

            if (!long.TryParse(Regex.Replace(routing, "[^0-9]", ""), out long routingNum))
                return;

            try
            {
                Util.MyHost = new BE.Host(fname, lname, email, phoneNum, bankBranch, routingNum);
                Util.Bl.CreateHost(Util.MyHost);
                MainWindow.Dialog($"Success! Your Host ID is {Util.MyHost.HostKey}. Use this number when you want to enter the hosting area.");
            }
            catch (Exception error)
            {
                MainWindow.Dialog(error.Message.ToString());
                return;
            }

            mainWindow.HostingFrame.Navigate(new HostMenu());
        }

        private void Return_To_Sign_In(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostSignIn());
        }

        //regex that matches disallowed text
        private static readonly Regex NumbersRegex = new Regex("[^0-9]+");

        private static bool IsTextAllowed(string text)
        {
            return !NumbersRegex.IsMatch(text);
        }

        private void Preview_Numbers(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void Pasting_Numbers(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
