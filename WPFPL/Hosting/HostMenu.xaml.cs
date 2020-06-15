using Project01_3693_dotNet5780;
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

        public MainWindow mainWindow;
        public HostMenu()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
        }

        private void View_Hosting_Units_Page(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new MyHostingUnits());
        }

        private void Return_To_Sign_In(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostSignIn());
        }

        private void View_Requests(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostRequests());
        }

        private void View_Orders(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostOrders());
        }
    }
}
