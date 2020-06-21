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
        public MainWindow mainWindow;
        public AdminMenu()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
        }

        private void View_Hosting_Units_Page(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminHostingUnits());
        }

        private void View_Requests(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminRequests());
        }

        private void View_Orders(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminOrders());
        }

        private void View_Hosts(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminHosts());
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminSignIn());
        }
    }
}
