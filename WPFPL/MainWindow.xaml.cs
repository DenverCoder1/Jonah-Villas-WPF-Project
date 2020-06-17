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

namespace WPFPL
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> DynamicCityList { get; set; }

        public TabItem CurrentTab { get; set; }

        /// <summary>
        /// Startup function
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            this.SizeChanged += OnWindowSizeChanged;
        }
        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 830)
                gAmenities.Style = (Style)System.Windows.Application.Current.Resources["MyMaterialDesignListBox"];
            else
                gAmenities.Style = (Style)System.Windows.Application.Current.Resources["MyMaterialDesignFilterChipListBox"];
        }

        /// <summary>
        /// Function to run when window is loaded
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentTab = Tab0;
            DynamicCityList = new ObservableCollection<string> { "Select a district." };
            gPrefCity.ItemsSource = DynamicCityList;
            HostingFrame.Navigate(new HostSignIn());
            AdminFrame.Navigate(new AdminMenu());
            MyDialog.IsOpen = false;

            /* DEBUG */
            DateTime entry = DateTime.Now.Date.AddDays(5);
            DateTime release = DateTime.Now.Date.AddDays(10);
            string fname = "Jonah";
            string lname = "Lawrence";
            string email = "jonah@villas.com";
            District region = District.TelAviv;
            City city = City.BneiBrak;
            BE.TypeOfPlace type = BE.TypeOfPlace.Apartment;
            Dictionary<Amenity, PrefLevel> amenities = new Dictionary<Amenity, PrefLevel>
            {
                [Amenity.TV] = PrefLevel.Required,
                [Amenity.Pool] = PrefLevel.NotInterested,
                [Amenity.Kitchen] = PrefLevel.Required,
                [Amenity.AirConditioning] = PrefLevel.Required,
                [Amenity.Breakfast] = PrefLevel.Required,
                [Amenity.ChildrensAttractions] = PrefLevel.Required,
                [Amenity.FreeParking] = PrefLevel.Required,
                [Amenity.PrivateBathroom] = PrefLevel.Required,
                [Amenity.SmokeAlarm] = PrefLevel.Required,
                [Amenity.Towels] = PrefLevel.Required,
                [Amenity.Workspace] = PrefLevel.Required
            };

            GuestRequest guest1 = new GuestRequest(entry, release, fname, lname, email, region, city, type, 2, 3, amenities);

            Util.Bl.CreateGuestRequest(guest1);

            entry = DateTime.Now.Date.AddDays(8);
            release = DateTime.Now.Date.AddDays(9);
            fname = "Yonah";
            lname = "Lawrence";
            email = "yonah@villas.com";
            region = District.Haifa;
            city = City.Hadera;
            type = BE.TypeOfPlace.PrivateRoom;
            amenities = new Dictionary<Amenity, PrefLevel>
            {
                [Amenity.TV] = PrefLevel.Required,
                [Amenity.Pool] = PrefLevel.Required,
                [Amenity.Kitchen] = PrefLevel.Required
            };

            GuestRequest guest2 = new GuestRequest(entry, release, fname, lname, email, region, city, type, 6, 8, amenities);

            Util.Bl.CreateGuestRequest(guest2);

            entry = DateTime.Now.Date.AddDays(11);
            release = DateTime.Now.Date.AddDays(17);
            fname = "Johan";
            lname = "Lawrence";
            email = "johan@villas.com";
            region = District.Jerusalem;
            city = City.BeitShemesh;
            type = BE.TypeOfPlace.EntireHome;
            amenities = new Dictionary<Amenity, PrefLevel>
            {
                [Amenity.Laundry] = PrefLevel.Required,
                [Amenity.Gym] = PrefLevel.Required,
                [Amenity.Kitchen] = PrefLevel.Required
            };

            GuestRequest guest3 = new GuestRequest(entry, release, fname, lname, email, region, city, type, 6, 8, amenities);

            Util.Bl.CreateGuestRequest(guest3);

            Host host1 = new Host("James", "Smith", "james@james.com", "6456346343", new BankBranch(), 543646545);

            Util.Bl.CreateHost(host1);

            Host host2 = new Host("Larry", "Page", "larry@googz.com", "4363466463", new BankBranch(), 6364636456);

            Util.Bl.CreateHost(host2);

            HostingUnit hostingUnit1 = new HostingUnit(host1, "myUnit");
            Util.Bl.CreateHostingUnit(hostingUnit1);

            HostingUnit hostingUnit2 = new HostingUnit(host2, "myUnit2");
            Util.Bl.CreateHostingUnit(hostingUnit2);

            Order order1 = new Order(hostingUnit1.HostingUnitKey, guest1.GuestRequestKey);

            Util.Bl.CreateOrder(order1);

            /* DEBUG */
        }

        /// <summary>
        /// Open custom dialog box with custom text
        /// </summary>
        /// <param name="text">Text to insert into box</param>
        public static void Dialog(string text, string tag = "", bool textBoxEnabled = false, bool comboBoxEnabled = false)
        {
            MainWindow mainWindow = Util.GetMainWindow();

            // hide / show input boxes
            mainWindow.MyDialogTextBox.Height = (!textBoxEnabled) ? (0 /* hidden */) : (double.NaN /* Auto */);
            mainWindow.MyDialogComboBox.Height = (!comboBoxEnabled) ? (0 /* hidden */) : (double.NaN /* Auto */);

            // set text and display
            mainWindow.MyDialogTextBox.Text = "";
            mainWindow.MyDialogComboBox.SelectedIndex = -1;
            mainWindow.MyDialog.Tag = tag;
            mainWindow.MyDialogText.Text = text;
            mainWindow.MyDialog.IsOpen = true;
        }

        /// <summary>
        /// Dialog closed handler
        /// </summary>
        private void Dialog_Closed(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            switch (MyDialog.Tag.ToString())
            {
                case "HostAddHostingUnit": HostHostingUnits.Add_Hosting_Unit_Named(MyDialogTextBox.Text); break;
                case "HostDeleteHostingUnit": HostHostingUnits.Confirm_Delete(MyDialogText.Text, MyDialogTextBox.Text); break;
                case "HostUpdateHostingUnit": HostHostingUnits.Update_Hosting_Unit_Name(MyDialogText.Text, MyDialogTextBox.Text); break;
                case "HostCreateOrder": HostRequests.Finish_Create_Order(MyDialogText.Text, MyDialogComboBox.SelectedItem); break;
                default: break;
            }
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
        /// Based on selected tab, hide borders of input controls from
        /// other tabs that should not be visible and refetch list data
        /// </summary>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // List of input controls in Tab 1
            List<Control> Tab1Controls = new List<Control>{
                gFirstName, gLastName, gEmail,
                gEntryDate, gReleaseDate, gPrefDistrict,
                gPrefCity, gNumAdults, gNumChildren, gPrefType
            };

            // Home
            if (CurrentTab != Tab0 && Tab0.IsSelected)
            {
                CurrentTab = Tab0;
                Util.SetTabControlsVisibility(Tab1Controls, false);
                if (HostSignUp.SignUpControls != null) HostSignUp.HideControls();
                if (HostSignIn.SignInControls != null) HostSignIn.HideControls();
            }
            // Guest Request
            else if (CurrentTab != Tab1 && Tab1.IsSelected)
            {
                CurrentTab = Tab1;
                Util.SetTabControlsVisibility(Tab1Controls, true);
                if (HostSignUp.SignUpControls != null) HostSignUp.HideControls();
                if (HostSignIn.SignInControls != null) HostSignIn.HideControls();
            }
            // Hosting
            else if (CurrentTab != Tab2 && Tab2.IsSelected)
            {
                CurrentTab = Tab2;
                Util.SetTabControlsVisibility(Tab1Controls, false);
                if (HostSignUp.SignUpControls != null) HostSignUp.ShowControls();
                if (HostSignIn.SignInControls != null) HostSignIn.ShowControls();
                HostRequests.Refresh();
                HostOrders.Refresh();
                HostHostingUnits.Refresh();
            }
            // Admin
            else if (CurrentTab != Tab3 && Tab3.IsSelected)
            {
                CurrentTab = Tab3;
                Util.SetTabControlsVisibility(Tab1Controls, false);
                if (HostSignUp.SignUpControls != null) HostSignUp.HideControls();
                if (HostSignIn.SignInControls != null) HostSignIn.HideControls();
                AdminRequests.Refresh();
                AdminOrders.Refresh();
                AdminHostingUnits.Refresh();
                AdminHosts.Refresh();
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
            List<string> update = Config.GetCities[district].ConvertAll(c => PascalCaseToText.Convert(c));
            // clear list
            DynamicCityList.Clear();
            // add cities in district
            foreach (string item in update)
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
                int numAdults = gNumAdults.SelectedIndex + 1; // No option for 0 adults
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

                Util.Bl.ValidateGuestForm(fname, lname, email, entry.ToString(), release.ToString(), districtObj, cityObj, numAdults, numChildren, prefTypeObj);

                Enum.TryParse(gPrefDistrict.SelectedItem.ToString().Replace(" ", ""), out District district);
                Enum.TryParse(gPrefCity.SelectedItem.ToString().Replace(" ", ""), out City city);
                Enum.TryParse(gPrefType.SelectedItem.ToString().Replace(" ", ""), out TypeOfPlace prefType);

                GuestRequest guest = new GuestRequest(entry, release, fname, lname, email, district, city, prefType, numAdults, numChildren, amenities);

                Util.Bl.CreateGuestRequest(guest);

                Dialog("Success! Your request has been added.");
            }
            catch (InvalidDataException error)
            {
                Dialog(error.Message.ToString());
            }
        }
    }
}
