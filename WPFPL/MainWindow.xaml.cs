using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private static IBL MyBL;

        public GuestRequest GuestReq { get; set; }

        public ObservableCollection<string> DynamicCityList { get; set; }

        /// <summary>
        /// Startup function
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// Function to run when window is loaded
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DynamicCityList = new ObservableCollection<string> { "Select a district." };
            gPrefCity.ItemsSource = DynamicCityList;
            MyBL = BL_Imp.GetBL();
            GuestReq = new GuestRequest();
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

        /// <summary>
        /// Set all input control borders from a given tab
        /// to be visible or not visible
        /// </summary>
        private void SetTabControlsVisibilityByTab(List<Control> TabControls, bool visible)
        {
            foreach (Control ctrl in TabControls)
            {
                ctrl.Width = visible ? Config.CONTROL_WIDTH : 0;
                ctrl.BorderThickness = visible ? new Thickness(0, 0, 0, 1) : new Thickness(0);
            }
        }

        /// <summary>
        /// Based on selected tab, hide borders of input controls
        /// from other tabs that should not be visible
        /// </summary>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // List of input controls in Tab 1
            List<Control> Tab1Controls = new List<Control>{
                gFirstName, gLastName, gEmail,
                gEntryDate, gReleaseDate, gPrefDistrict,
                gPrefCity, gNumAdults, gNumChildren
            };

            if (Tab0.IsSelected) {
                SetTabControlsVisibilityByTab(Tab1Controls, false);
            }
            else if (Tab1.IsSelected)
            {
                SetTabControlsVisibilityByTab(Tab1Controls, true);
            }
            else if (Tab2.IsSelected)
            {
                SetTabControlsVisibilityByTab(Tab1Controls, false);
            }
            else if (Tab3.IsSelected)
            {
                SetTabControlsVisibilityByTab(Tab1Controls, false);
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
                    DynamicCityList.Clear();

                    foreach (var x in update)
                    {
                        DynamicCityList.Add(x);
                    }
                    break;
                }
            }
            if (DynamicCityList.Count == 0)
            {
                DynamicCityList.Add("Select a district.");
            }
            return;
        }

        private void Submit_Request_Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime entry = DateTime.Parse(gEntryDate.SelectedDate.ToString());
            DateTime release = DateTime.Parse(gReleaseDate.SelectedDate.ToString());
            string fname = gFirstName.Text.ToString();
            string lname = gLastName.Text.ToString();
            string email = gEmail.Text.ToString();
            Enum.TryParse(gPrefDistrict.SelectedItem.ToString().Replace(" ", ""), out District district);
            Enum.TryParse(gPrefCity.SelectedItem.ToString().Replace(" ", ""), out City city);
            int numAdults = gNumAdults.SelectedIndex + 1;
            int numChildren = gNumChildren.SelectedIndex + 1;
            System.Collections.IList selectedAmenities = gAmenities.SelectedItems;
            Dictionary<Amenity, PrefLevel> amenities = new Dictionary<Amenity, PrefLevel>();
            foreach (object item in selectedAmenities)
            {
                amenities[(Amenity)item] = PrefLevel.Required;
            }

            GuestRequest guest = new GuestRequest(entry, release, fname, lname, email, district, city, numAdults, numChildren, amenities);

            MyBL.CreateGuestRequest(guest);
        }
    }
}
