using Project01_3693_dotNet5780;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for HostOrders.xaml
    /// </summary>
    public partial class HostOrders : Page
    {
        public MainWindow mainWindow;

        public ObservableCollection<string> OrdersCollection { get; set; }
        public HostOrders()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            OrdersCollection = new ObservableCollection<string> { "Test" };
            Orders.ItemsSource = OrdersCollection;
        }

        private void Return_To_Options(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostChoices());
        }

        private void Update_Order(object sender, RoutedEventArgs e)
        {

        }
    }
}
