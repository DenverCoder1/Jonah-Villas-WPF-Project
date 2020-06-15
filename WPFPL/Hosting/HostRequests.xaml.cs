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
    /// Interaction logic for HostRequests.xaml
    /// </summary>
    public partial class HostRequests : Page
    {
        public MainWindow mainWindow;

        public ObservableCollection<string> RequestCollection { get; set; }
        public HostRequests()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            RequestCollection = new ObservableCollection<string> { "Test" };
            Requests.ItemsSource = RequestCollection;
        }

        private void Return_To_Options(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostMenu());
        }

        private void Create_Order(object sender, RoutedEventArgs e)
        {

        }
    }
}
