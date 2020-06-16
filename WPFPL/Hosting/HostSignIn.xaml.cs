using BE;
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
using System.Text.RegularExpressions;
using BL;

namespace WPFPL
{
    /// <summary>
    /// Interaction logic for HostSignIn.xaml
    /// </summary>
    public partial class HostSignIn : Page
    {
        public MainWindow mainWindow;

        public static List<Control> SignInControls;
        public HostSignIn()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            SignInControls = new List<Control>{
                HostID
            };
            ShowControls();
        }

        public static void ShowControls()
        {
            Util.SetTabControlsVisibility(SignInControls, true);
        }

        public static void HideControls()
        {
            Util.SetTabControlsVisibility(SignInControls, false);
        }

        private void Host_Enter_Button_Click(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(HostID.Text, out long hKey))
            {
                Host host = Util.Bl.GetHost(hKey);
                if (host == null)
                {
                    MainWindow.Dialog("Host ID does not exist.");
                    return;
                }
                Util.MyHost = host;
                mainWindow.HostingFrame.Navigate(new HostMenu());
            }
            else
            {
                MainWindow.Dialog("Host ID is invalid.");
            }
        }

        private void Host_Sign_Up_Button_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostSignUp());
        }

        private void HostID_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;

            // Enter key pressed
            e.Handled = true;
            Host_Enter_Button_Click(sender, e);
        }

        private static readonly Regex NumbersRegex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !NumbersRegex.IsMatch(text);
        }

        // Use the DataObject.Pasting Handler 
        private void HostID_Pasting(object sender, DataObjectPastingEventArgs e)
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

        private void HostID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
    }
}
