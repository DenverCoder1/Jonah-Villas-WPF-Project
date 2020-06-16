using BE;
using BL;
using Project01_3693_dotNet5780;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for AdminRequests.xaml
    /// </summary>
    public partial class AdminRequests : Page
    {
        public MainWindow mainWindow;

        private static IBL MyBL;

        public static ObservableCollection<string> RequestCollection { get; set; }

        public AdminRequests()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            MyBL = BL_Imp.GetBL();
            RequestCollection = new ObservableCollection<string>();
            Requests.ItemsSource = RequestCollection;
            Refresh();
        }

        public static void Refresh()
        {
            if (RequestCollection != null)
            {
                RequestCollection.Clear();
                foreach (BE.GuestRequest item in MyBL.GetGuestRequests())
                {
                    RequestCollection.Add(item.ToString());
                }
            }
        }

        private void Return_To_Menu(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminMenu());
        }

        private void Create_Order(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
