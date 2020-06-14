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
using System.Xaml;
using BE;
using BL;

namespace Project01_3693_dotNet5780
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const bool DEBUG_MODE = true;

        private static IBL MyBL;

        public GuestRequest myGuestRequest { get; set; }

        public ObservableCollection<string> ObsCityList { get; set; } = new ObservableCollection<string> { "Select a district." };

        public static List<Control> Tab1Controls;

        /// <summary>
        /// Startup function
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            gPrefCity.ItemsSource = ObsCityList;

            Tab1Controls = new List<Control>{
                gFirstName,
                gLastName,
                gEmail,
                gEntryDate,
                gReleaseDate,
                gPrefDistrict,
                gPrefCity,
                gNumAdults,
                gNumChildren
            };

            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// Temporary function for using home page as log
        /// </summary>
        /// <param name="text">Text to log</param>
        private void Log(string text)
        {
            //if (DEBUG_MODE) DebugLog.Text += text + "\n";
        }

        /// <summary>
        /// Function to run when window is loaded
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MyBL = BL_Imp.GetBL();
            myGuestRequest = new GuestRequest();
        }

        /// <summary>
        /// Change to a different tab in Main Tab Control
        /// Modifies the SelectedIndex attribute based on sender's tag
        /// </summary>
        private void ChangeTab(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
                MainTabControl.SelectedIndex =
                    int.Parse(((Button)sender).Tag.ToString()))
            );
        }

        private void SetTab1ControlsVisibility(bool visible)
        {
            foreach (Control ctrl in Tab1Controls)
            {
                ctrl.Width = visible ? Config.CONTROL_WIDTH : 0;
                ctrl.BorderThickness = visible ? new Thickness(0, 0, 0, 1) : new Thickness(0);
            }
        }

        

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tab0.IsSelected) {
                SetTab1ControlsVisibility(false);
            }
            else if (Tab1.IsSelected)
            {
                SetTab1ControlsVisibility(true);
            }
            else if (Tab2.IsSelected)
            {
                SetTab1ControlsVisibility(false);
            }
            else if (Tab3.IsSelected)
            {
                SetTab1ControlsVisibility(false);
            }
        }

        /// <summary>
        /// Update City List when District List changed
        /// </summary>
        private void UpdateCityList(object sender, SelectionChangedEventArgs e)
        {
            string selectedDistrict = ((ComboBox)sender).SelectedItem.ToString();
            foreach (KeyValuePair<District, string> item in Config.DistrictNames)
            {
                if (selectedDistrict == item.Key.ToString())
                {
                    List<string> update = Config.GetCities[item.Key].ConvertAll(c => Config.CityNames[c]);
                    ObsCityList.Clear();

                    foreach (var x in update)
                    {
                        ObsCityList.Add(x);
                    }
                    break;
                }
            }
            if (ObsCityList.Count == 0)
            {
                ObsCityList.Add("Select a district.");
            }
            return;
        }
    }
}
