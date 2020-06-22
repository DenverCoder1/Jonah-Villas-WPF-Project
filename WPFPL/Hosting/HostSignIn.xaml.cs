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
        private static readonly MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        public HostSignIn()
        {
            InitializeComponent();
            if (MainWindow.LoggedInHost != null && MainWindow.LoggedInHost.HostKey > 0)
                HostID.Text = MainWindow.LoggedInHost.HostKey.ToString();
        }

        /// <summary>
        /// Check ID and go to menu if valid
        /// </summary>
        private void Host_Enter_Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(HostID.Text))
            {
                MainWindow.Dialog("You have not entered your host ID.");
            }
            else if (long.TryParse(HostID.Text, out long hKey))
            {
                try
                {
                    Host host = MainWindow.Bl.GetHost(hKey);
                    if (host == null)
                    {
                        MainWindow.Dialog("Host ID does not exist.");
                        return;
                    }
                    MainWindow.LoggedInHost = host;
                    mainWindow.HostingFrame.Navigate(new HostMenu());
                }
                catch (Exception error)
                {
                    mainWindow.MySnackbar.MessageQueue.Enqueue(error.Message);
                    return;
                }
            }
            else
            {
                MainWindow.Dialog("Host ID is invalid.");
            }
        }

        /// <summary>
        /// Go to sign up page
        /// </summary>
        private void Host_Sign_Up_Button_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostSignUp());
        }

        /// <summary>
        /// Detect enter key pressed in the ID field
        /// Submit form if enter pressed
        /// </summary>
        private void HostID_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;

            // Enter key pressed
            e.Handled = true;
            Host_Enter_Button_Click(sender, e);
        }

        //regex that matches disallowed text for ID field
        private static readonly Regex NumbersRegex = new Regex("[^0-9]+"); 

        // return whether string contains all numbers
        private static bool IsTextAllowed(string text)
        {
            return !NumbersRegex.IsMatch(text);
        }

        /// <summary>
        ///  Use the DataObject.Pasting Handler to disallow
        ///  pasting text that contains non-numeric characters
        /// </summary>
        private void HostID_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
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

        /// <summary>
        /// Display text only if it is numeric
        /// </summary>
        private void HostID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
    }
}
