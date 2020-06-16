using BL;
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

namespace WPFPL.Admin
{
    /// <summary>
    /// Interaction logic for AdminHostingUnits.xaml
    /// </summary>
    public partial class AdminHostingUnits : Page
    {
        public MainWindow mainWindow;

        public static ObservableCollection<string> HostingUnitCollection { get; set; }

        public AdminHostingUnits()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            HostingUnitCollection = new ObservableCollection<string>();
            HostingUnits.ItemsSource = HostingUnitCollection;
            Refresh();
        }

        public static void Refresh()
        {
            if (HostingUnitCollection != null)
            {
                HostingUnitCollection.Clear();
                foreach (BE.HostingUnit item in Util.Bl.GetHostingUnits())
                {
                    HostingUnitCollection.Add(item.ToString());
                }
            }
        }

        private void Return_To_Menu(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminMenu());
        }
    }
}
