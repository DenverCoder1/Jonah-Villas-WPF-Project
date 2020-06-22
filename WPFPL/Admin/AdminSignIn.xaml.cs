using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace WPFPL.Admin
{
    /// <summary>
    /// Interaction logic for AdminSignIn.xaml
    /// </summary>
    public partial class AdminSignIn : Page
    {
        private readonly MainWindow mainWindow;

        public AdminSignIn()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
        }

        /// <summary>
        /// Submit form button click
        /// Check login info and go to menu if valid
        /// </summary>
        private void Admin_Enter_Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AdminUsername.Text))
            {
                MainWindow.Dialog("You have not entered your username.");
                return;
            }
            else if(string.IsNullOrEmpty(AdminPassword.Text))
            {
                MainWindow.Dialog("You have not entered your password.");
                return;
            }
            else if (AdminUsername.Text == ConfigurationManager.AppSettings["ADMIN_USERNAME"]
                && AdminPassword.Text == ConfigurationManager.AppSettings["ADMIN_PASSWORD"])
            {
                mainWindow.AdminFrame.Navigate(new AdminMenu());
                return;
            }
            else
            {
                MainWindow.Dialog("Username or password is incorrect.");
                return;
            }
        }

        /// <summary>
        /// submit form when enter key pressed from password box
        /// </summary>
        private void AdminPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;

            // Enter key pressed
            e.Handled = true;
            Admin_Enter_Button_Click(sender, e);
        }
    }
}
