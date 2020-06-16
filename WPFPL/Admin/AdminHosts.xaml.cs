using BL;
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

namespace WPFPL.Admin
{
    /// <summary>
    /// Interaction logic for AdminHosts.xaml
    /// </summary>
    public partial class AdminHosts : Page
    {
        public MainWindow mainWindow;

        public static ObservableCollection<string> HostCollection { get; set; }

        public AdminHosts()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            HostCollection = new ObservableCollection<string>();
            Hosts.ItemsSource = HostCollection;
            Refresh();
        }

        public static void Refresh()
        {
            IBL Bl = BL_Imp.GetBL();
            if (HostCollection != null)
            {
                HostCollection.Clear();
                foreach (BE.Host item in Bl.GetHosts())
                {
                    HostCollection.Add(item.ToString());
                }
            }
        }

        private void Return_To_Menu(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminMenu());
        }
    }
}
