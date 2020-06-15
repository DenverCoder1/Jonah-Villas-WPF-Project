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
    /// Interaction logic for MyHostingUnits.xaml
    /// </summary>
    public partial class MyHostingUnits : Page
    {
        public MainWindow mainWindow;

        public ObservableCollection<string> HostingUnitCollection { get; set; }
        public MyHostingUnits()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            HostingUnitCollection = new ObservableCollection<string> { "Test" };
            HostingUnits.ItemsSource = HostingUnitCollection;
        }

        private void Return_To_Options(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostChoices());
        }

        private void Update_Hosting_Unit(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_Hosting_Unit(object sender, RoutedEventArgs e)
        {

        }

        private void Add_Hosting_Unit(object sender, RoutedEventArgs e)
        {

        }
    }
}
