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

namespace WPFPL.Admin
{
    /// <summary>
    /// Interaction logic for AdminMenu.xaml
    /// </summary>
    public partial class AdminMenu : Page
    {
        private static readonly MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        public AdminMenu()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Go to hosting unit list view
        /// </summary>
        private void View_Hosting_Units_Page(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminHostingUnits());
        }

        /// <summary>
        /// Go to request list view
        /// </summary>
        private void View_Requests(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminRequests());
        }

        /// <summary>
        /// Go to order list view
        /// </summary>
        private void View_Orders(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminOrders());
        }

        /// <summary>
        /// Go to host list view
        /// </summary>
        private void View_Hosts(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminHosts());
        }

        /// <summary>
        /// Return to sign in page when exit clicked
        /// </summary>
        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminSignIn());
        }
    }
}
