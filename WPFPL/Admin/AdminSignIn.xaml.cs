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
        public MainWindow mainWindow;
        public AdminSignIn()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
        }

        private void Admin_Enter_Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(AdminUsername.Text))
            {
                MainWindow.Dialog("You have not entered your username.");
                return;
            }
            else if(String.IsNullOrEmpty(AdminPassword.Text))
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
    }
}
