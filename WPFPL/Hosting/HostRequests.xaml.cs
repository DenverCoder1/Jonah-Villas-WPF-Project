using WPFPL;
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
using BL;

namespace WPFPL
{
    /// <summary>
    /// Interaction logic for HostRequests.xaml
    /// </summary>
    public partial class HostRequests : Page
    {
        public MainWindow mainWindow;

        public static ObservableCollection<string> RequestCollection { get; set; }
        public HostRequests()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            RequestCollection = new ObservableCollection<string>();
            Requests.ItemsSource = RequestCollection;
            Refresh();
        }

        public static void Refresh()
        {
            IBL Bl = BL_Imp.GetBL();
            if (RequestCollection != null)
            {
                RequestCollection.Clear();
                foreach (BE.GuestRequest item in Bl.GetGuestRequests())
                {
                    RequestCollection.Add(item.ToString());
                }
            }
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
