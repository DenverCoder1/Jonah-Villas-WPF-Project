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

namespace WPFPL
{
    /// <summary>
    /// Interaction logic for HostSignUp.xaml
    /// </summary>
    public partial class HostSignUp : Page
    {
        public MainWindow mainWindow;

        public static List<Control> SignUpControls;

        public static ObservableCollection<string> BankCollection { get; set; }

        public HostSignUp()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BankCollection = new ObservableCollection<string>();
            hBranch.ItemsSource = BankCollection;
            ListBanks();
            SignUpControls = new List<Control>{
                hFirstName, hLastName, hEmail, hPhone,
                hBranch, hRoutingNumber
            };
            ShowControls();
        }

        public static void ListBanks()
        {
            if (BankCollection != null)
            {
                BankCollection.Clear();
                foreach (BankBranch item in Util.Bl.GetBankBranches())
                {
                    BankCollection.Add(item.ToString());
                }
            }
        }

        public static void ShowControls()
        {
            Util.SetTabControlsVisibility(SignUpControls, true);
        }

        public static void HideControls()
        {
            Util.SetTabControlsVisibility(SignUpControls, false);
        }

        private void Create_Account(object sender, RoutedEventArgs e)
        {
            string fname = hFirstName.Text;
            string lname = hLastName.Text;
            string email = hEmail.Text;
            string phone = hPhone.Text;
            string branch = hBranch.Text;
            BankBranch bankBranch = Util.Bl.GetBankBranches()[0];
            string routing = hRoutingNumber.Text;

            try
            {
                Util.Bl.ValidateHostSignUp(fname, lname, email, phone, branch, routing);
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
