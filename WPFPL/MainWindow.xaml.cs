using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using WPFPL;
using WPFPL.Admin;

namespace Project01_3693_dotNet5780
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static IBL MyBL;

        public ObservableCollection<string> DynamicCityList { get; set; }

        public TabItem CurrentTab { get; set; }

        public static Host MyHost { get; set; }

        /// <summary>
        /// Startup function
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            MyBL = BL_Imp.GetBL();
            DynamicCityList = new ObservableCollection<string> { "Select a district." };
            CurrentTab = Tab0;
            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// Function to run when window is loaded
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            gPrefCity.ItemsSource = DynamicCityList;
            HostingFrame.Navigate(new HostSignIn());
            AdminFrame.Navigate(new AdminMenu());
            MyDialog.IsOpen = false;

            DateTime entry = DateTime.Now.Date.AddDays(5);
            DateTime release = DateTime.Now.Date.AddDays(10);
            string fname = "Jonah";
            string lname = "Lawrence";
            string email = "jonah@google.com";
            District region = District.TelAviv;
            City city = City.BneiBrak;
            BE.TypeOfPlace type = BE.TypeOfPlace.Apartment;
            Dictionary<Amenity, PrefLevel> amenities = new Dictionary<Amenity, PrefLevel>
            {
                [Amenity.TV] = PrefLevel.Required,
                [Amenity.Pool] = PrefLevel.NotInterested,
                [Amenity.Kitchen] = PrefLevel.Required
            };

            GuestRequest guest = new GuestRequest(entry, release, fname, lname, email, region, city, type, 6, 8, amenities);

            MyBL.CreateGuestRequest(guest);
        }

        /// <summary>
        /// Open custom dialog box with custom text
        /// </summary>
        /// <param name="text">Text to insert into box</param>
        private void Dialog(string text)
        {
            MyDialogText.Text = text;
            MyDialog.IsOpen = true;
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
        /// Based on selected tab, hide borders of input controls
        /// from other tabs that should not be visible
        /// </summary>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // List of input controls in Tab 1
            List<Control> Tab1Controls = new List<Control>{
                gFirstName, gLastName, gEmail,
                gEntryDate, gReleaseDate, gPrefDistrict,
                gPrefCity, gNumAdults, gNumChildren, gPrefType
            };

            if (CurrentTab != Tab0 && Tab0.IsSelected)
            {
                CurrentTab = Tab0;
                Util.SetTabControlsVisibility(Tab1Controls, false);
                if (HostSignUp.SignUpControls != null) HostSignUp.HideControls();
                if (HostSignIn.SignInControls != null) HostSignIn.HideControls();
            }
            else if (CurrentTab != Tab1 && Tab1.IsSelected)
            {
                CurrentTab = Tab1;
                Util.SetTabControlsVisibility(Tab1Controls, true);
                if (HostSignUp.SignUpControls != null) HostSignUp.HideControls();
                if (HostSignIn.SignInControls != null) HostSignIn.HideControls();
            }
            else if (CurrentTab != Tab2 && Tab2.IsSelected)
            {
                CurrentTab = Tab2;
                Util.SetTabControlsVisibility(Tab1Controls, false);
                if (HostSignUp.SignUpControls != null) HostSignUp.ShowControls();
                if (HostSignIn.SignInControls != null) HostSignIn.ShowControls();
            }
            else if (CurrentTab != Tab3 && Tab3.IsSelected)
            {
                CurrentTab = Tab3;
                Util.SetTabControlsVisibility(Tab1Controls, false);
                if (HostSignUp.SignUpControls != null) HostSignUp.HideControls();
                if (HostSignIn.SignInControls != null) HostSignIn.HideControls();
                AdminRequests.Refresh();
            }
        }

        /// <summary>
        /// Update City List when District List changed
        /// </summary>
        private void UpdateCityList(object sender, SelectionChangedEventArgs e)
        {
            // get selected district
            if (((ComboBox)sender).SelectedItem == null) return;
            string selectedDistrict = ((ComboBox)sender).SelectedItem.ToString();
            if (!Enum.TryParse(gPrefDistrict.SelectedItem.ToString().Replace(" ", ""), out District district)) return;
            // get list of cities in district from config
            List<string> update = Config.GetCities[district].ConvertAll(c => CamelCaseConverter.Convert(c));
            // clear list
            DynamicCityList.Clear();
            // add cities in district
            foreach (var item in update)
            {
                DynamicCityList.Add(item);
            }
            // if list is empty, add item that says to select a district first
            if (DynamicCityList.Count == 0)
            {
                DynamicCityList.Add("Select a district.");
            }
            return;
        }

        /// <summary>
        /// Create guest request from submitted info
        /// </summary>
        private void Submit_Request_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fname = gFirstName.Text.ToString();
                string lname = gLastName.Text.ToString();
                string email = gEmail.Text.ToString();
                DateTime.TryParse(gEntryDate.SelectedDate.ToString(), out DateTime entry);
                DateTime.TryParse(gReleaseDate.SelectedDate.ToString(), out DateTime release);
                object districtObj = gPrefDistrict.SelectedItem;
                object cityObj = gPrefCity.SelectedItem;
                int numAdults = gNumAdults.SelectedIndex;
                int numChildren = gNumChildren.SelectedIndex;
                object prefTypeObj = gPrefType.SelectedItem;
                System.Collections.IList selectedAmenities = gAmenities.SelectedItems;
                Dictionary<Amenity, PrefLevel> amenities = new Dictionary<Amenity, PrefLevel>();
                foreach (Amenity amenity in Enum.GetValues(typeof(Amenity)))
                {
                    if (selectedAmenities.IndexOf(amenity) > -1)
                        amenities[amenity] = PrefLevel.Required;
                    else
                        amenities[amenity] = PrefLevel.NotInterested;
                }

                MyBL.ValidateGuestForm(fname, lname, email, entry.ToString(), release.ToString(), districtObj, cityObj, numAdults, numChildren, prefTypeObj);

                Enum.TryParse(gPrefDistrict.SelectedItem.ToString().Replace(" ", ""), out District district);
                Enum.TryParse(gPrefCity.SelectedItem.ToString().Replace(" ", ""), out City city);
                Enum.TryParse(gPrefType.SelectedItem.ToString().Replace(" ", ""), out TypeOfPlace prefType);

                GuestRequest guest = new GuestRequest(entry, release, fname, lname, email, district, city, prefType, numAdults, numChildren, amenities);

                MyBL.CreateGuestRequest(guest);

                Dialog("Success! Your request has been added.");
            }
            catch (InvalidDataException error)
            {
                Dialog(error.Message.ToString());
            }
        }
    }
}
