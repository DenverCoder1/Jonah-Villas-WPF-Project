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

namespace WPFPL
{
    /// <summary>
    /// Interaction logic for HostMenu.xaml
    /// </summary>
    public partial class HostMenu : Page
    {

        private readonly MainWindow mainWindow;

        public HostMenu()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            YourHostID.Text = $"Your host ID is {Util.MyHost.HostKey}";
        }

        /// <summary>
        /// Go to hosting unit list
        /// </summary>
        private void View_Hosting_Units_Page(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostHostingUnits());
        }

        /// <summary>
        /// Go to requests list
        /// </summary>
        private void View_Requests(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostRequests());
        }

        /// <summary>
        /// Go to orders list
        /// </summary>
        private void View_Orders(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostOrders());
        }

        /// <summary>
        /// Go back to host sign in page
        /// </summary>
        private void Return_To_Sign_In(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostSignIn());
        }
    }
}
