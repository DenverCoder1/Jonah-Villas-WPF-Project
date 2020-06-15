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
using WPFPL;

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
            HostingFrame.Navigate(new HostSignIn());
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

            if (Tab0.IsSelected) {
                Util.SetTabControlsVisibility(Tab1Controls, false);
                if (HostSignUp.SignUpControls != null) HostSignUp.HideControls();
            }
            else if (Tab1.IsSelected)
            {
                Util.SetTabControlsVisibility(Tab1Controls, true);
                if (HostSignUp.SignUpControls != null) HostSignUp.HideControls();
            }
            else if (Tab2.IsSelected)
            {
                Util.SetTabControlsVisibility(Tab1Controls, false);
                if (HostSignUp.SignUpControls != null) HostSignUp.ShowControls();
            }
            else if (Tab3.IsSelected)
            {
                Util.SetTabControlsVisibility(Tab1Controls, false);
                if (HostSignUp.SignUpControls != null) HostSignUp.HideControls();
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
        /// Check if an email address is valid
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateGuestForm()
        {
            if (gFirstName.Text.ToString().Length < 2)
            {
                MessageBox.Show("First name must contain at least 2 letters.");
                return false;
            }
            else if (!gFirstName.Text.ToString().All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("First name must only contain letters and whitespace.");
                return false;
            }
            else if(gLastName.Text.ToString().Length < 2)
            {
                MessageBox.Show("Last name must contain at least 2 letters.");
                return false;
            }
            else if (!gLastName.Text.ToString().All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("Last name must only contain letters and whitespace.");
                return false;
            }
            else if (!IsValidEmail(gEmail.Text.ToString()))
            {
                MessageBox.Show("Email address is not valid.");
                return false;
            }
            else if (!DateTime.TryParse(gEntryDate.SelectedDate.ToString(), out DateTime entry))
            {
                MessageBox.Show("Entry date is not valid.");
                return false;
            }
            else if (DateTime.Compare(entry.Date, DateTime.Now.Date) < 0)
            {
                MessageBox.Show("Entry date must not be before today's date.");
                return false;
            }
            else if (!DateTime.TryParse(gReleaseDate.SelectedDate.ToString(), out DateTime release))
            {
                MessageBox.Show("Departure date is not valid.");
                return false;
            }
            else if (DateTime.Compare(entry.Date, release.Date) >= 0)
            {
                MessageBox.Show("Entry date must be before departure date.");
                return false;
            }
            else if (DateTime.Compare(release.Date, DateTime.Now.Date.AddMonths(11)) > 0)
            {
                MessageBox.Show("Bookings can only be made up to 11 months in advance.");
                return false;
            }
            try
            {
                if (gPrefDistrict.SelectedItem != null)
                {
                    District _ = (District)Enum.Parse(typeof(District), gPrefDistrict.SelectedItem.ToString().Replace(" ", ""));
                }
                else
                {
                    MessageBox.Show("You have not selected a district.");
                    return false;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("District selection is not valid.");
                return false;
            }
            try
            {
                if (gPrefCity.SelectedItem != null)
                {
                    City _ = (City)Enum.Parse(typeof(City), gPrefCity.SelectedItem.ToString().Replace(" ", ""));
                }
                else
                {
                    MessageBox.Show("You have not selected a city.");
                    return false;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("City selection is not valid.");
                return false;
            }
            if (gNumAdults.SelectedIndex == -1)
            {
                MessageBox.Show("Please select the number of adults.");
                return false;
            }
            else if (gNumChildren.SelectedIndex == -1)
            {
                MessageBox.Show("Please select the number of children.");
                return false;
            }
            if (gNumAdults.SelectedIndex + gNumChildren.SelectedIndex == 0)
            {
                MessageBox.Show("Booking must be reserved for at least 1 person.");
                return false;
            }
            try
            {
                if (gPrefType.SelectedItem != null)
                {
                    TypeOfPlace _ = (TypeOfPlace)Enum.Parse(typeof(TypeOfPlace), gPrefType.SelectedItem.ToString().Replace(" ", ""));
                }
                else
                {
                    MessageBox.Show("You have not selected a type of place.");
                    return false;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Type of place selection is not valid.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create guest request from submitted info
        /// </summary>
        private void Submit_Request_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateGuestForm())
                return;
            string fname = gFirstName.Text.ToString();
            string lname = gLastName.Text.ToString();
            string email = gEmail.Text.ToString();
            DateTime.TryParse(gEntryDate.SelectedDate.ToString(), out DateTime entry);
            DateTime.TryParse(gReleaseDate.SelectedDate.ToString(), out DateTime release);
            Enum.TryParse(gPrefDistrict.SelectedItem.ToString().Replace(" ", ""), out District district);
            Enum.TryParse(gPrefCity.SelectedItem.ToString().Replace(" ", ""), out City city);
            int numAdults = gNumAdults.SelectedIndex;
            int numChildren = gNumChildren.SelectedIndex;
            Enum.TryParse(gPrefType.SelectedItem.ToString().Replace(" ", ""), out TypeOfPlace type);
            System.Collections.IList selectedAmenities = gAmenities.SelectedItems;
            Dictionary<Amenity, PrefLevel> amenities = new Dictionary<Amenity, PrefLevel>();
            foreach (Amenity amenity in Enum.GetValues(typeof(Amenity)))
            {
                if (selectedAmenities.IndexOf(amenity) > -1)
                    amenities[amenity] = PrefLevel.Required;
                else
                    amenities[amenity] = PrefLevel.NotInterested;
            }

            GuestRequest guest = new GuestRequest(entry, release, fname, lname, email, district, city, type, numAdults, numChildren, amenities);

            MyBL.CreateGuestRequest(guest);

            MessageBox.Show("Success! Your request has been added.");
        }
    }
}
