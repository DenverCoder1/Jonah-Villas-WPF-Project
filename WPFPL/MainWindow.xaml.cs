using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using BE;
using BL;
using WPFPL.Admin;

namespace WPFPL
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Get the instance of the BL
        public static IBL Bl = FactoryBL.GetBL();

        // Host which is currently logged in
        public static Host LoggedInHost { get; set; }

        // current visible tab
        private TabItem CurrentTab { get; set; }

        // Number of units available for current guest request
        private int UnitsAvailable { get; set; }

        // Current guest request
        private GuestRequest Guest { get; set; }

        // list of cities in selected district
        private ObservableCollection<string> DynamicCityList { get; set; }

        // Selected button from dialog host
        private string MyDialogResult { get; set; }

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
            CurrentTab = Tab0;
            DynamicCityList = new ObservableCollection<string> { "Select district first." };
            gPrefCity.ItemsSource = DynamicCityList;
            HostingFrame.Navigate(new HostSignIn());
            AdminFrame.Navigate(new AdminSignIn());
            MyDialog.IsOpen = false;
            this.SizeChanged += ChooseAmenityListBoxStyle;
            this.DataContext = this;

            // start thread for expiring orders daily
            BL.OrderExpiration.StartJob();
        }

        /// <summary>
        /// Open custom dialog box with custom text
        /// </summary>
        /// <param name="text">Text to insert into box</param>
        public static void Dialog(string text, string tag = "", object textBox = null, object combo1 = null, object combo2 = null, object checkbox = null, object combo3 = null, object listbox = null, bool cancelButton = false)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            // hide / show input boxes
            mainWindow.MyDialogTextBox.Height = (textBox == null) ? (0 /* hidden */) : (double.NaN /* Auto */);
            mainWindow.MyDialogComboBox1.Height = (combo1 == null) ? (0 /* hidden */) : (double.NaN /* Auto */);
            mainWindow.MyDialogComboBox2.Height = (combo2 == null) ? (0 /* hidden */) : (double.NaN /* Auto */);
            mainWindow.MyDialogCheckbox.Height = (checkbox == null) ? (0 /* hidden */) : (double.NaN /* Auto */);
            mainWindow.MyDialogComboBox3.Height = (combo3 == null) ? (0 /* hidden */) : (double.NaN /* Auto */);
            mainWindow.MyDialogListBox.Height = (listbox == null) ? (0 /* hidden */) : (double.NaN /* Auto */);
            mainWindow.DialogCancelButton.Width = (cancelButton == false) ? (0 /* hidden */) : (double.NaN /* Auto */);

            mainWindow.MyDialogTextBox.Margin = (textBox == null) ? new Thickness(0) : new Thickness(0, 6, 20, 6);
            mainWindow.MyDialogComboBox1.Margin = (combo1 == null) ? new Thickness(0) : new Thickness(0, 6, 20, 6);
            mainWindow.MyDialogComboBox2.Margin = (combo2 == null) ? new Thickness(0) : new Thickness(0, 6, 20, 6);
            mainWindow.MyDialogCheckbox.Margin = (checkbox == null) ? new Thickness(0) : new Thickness(0, 10, 0, 0);
            mainWindow.MyDialogComboBox3.Margin = (combo3 == null) ? new Thickness(0) : new Thickness(0, 6, 20, 6);

            // switch to full size box if list box is shown
            mainWindow.MyDialogContent.Width = (listbox == null) ? (250 /* normal size */) : (double.NaN /* Auto */);

            // set text and display
            if (textBox != null) { mainWindow.MyDialogTextBox.Text = textBox.ToString(); }
            if (combo1 != null) { mainWindow.MyDialogComboBox1.SelectedItem = ((string)combo1 != "") ? combo1.ToString() : null; }
            if (combo2 != null) { mainWindow.MyDialogComboBox2.SelectedItem = ((string)combo2 != "") ? combo2.ToString() : null; }
            if (checkbox != null) { mainWindow.MyDialogCheckbox.IsChecked = (bool)checkbox; }
            if (combo3 != null) { mainWindow.MyDialogComboBox3.SelectedItem = ((string)combo3 != "") ? combo3.ToString() : null; }
            if (listbox != null) { mainWindow.MyDialogListBox.SelectedItems.Clear(); }
            mainWindow.MyDialog.Tag = tag;
            mainWindow.MyDialogText.Text = text;
            mainWindow.MyDialog.IsOpen = true;
        }

        /// <summary>
        /// Dialog closed handler
        /// </summary>
        private void Dialog_Closed(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (MyDialog.Tag != null)
            {
                switch (MyDialog.Tag.ToString())
                {
                    case "HostAddHostingUnit": ((HostHostingUnits)HostingFrame.Content).Finish_Add_Hosting_Unit(); break;
                    case "HostDeleteHostingUnit": ((HostHostingUnits)HostingFrame.Content).Confirm_Delete(MyDialogText.Text, MyDialogCheckbox.IsChecked); break;
                    case "HostUpdateHostingUnit": ((HostHostingUnits)HostingFrame.Content).Finish_Update_Hosting_Unit(MyDialogText.Text, MyDialogTextBox.Text); break;
                    case "AdminUpdateBankClearance": ((AdminHosts)AdminFrame.Content).FinishUpdateBankClearance(MyDialogText.Text, MyDialogComboBox1.SelectedItem.ToString()); break;
                    case "HostCreateOrder": ((HostRequests)HostingFrame.Content).Finish_Create_Order(MyDialogText.Text, MyDialogComboBox1.SelectedItem); break;
                    case "HostUpdateOrder": ((HostOrders)HostingFrame.Content).Finish_Update_Order(MyDialogText.Text, MyDialogComboBox1.SelectedItem); break;
                    case "AdminUpdateOrder": ((AdminOrders)AdminFrame.Content).Finish_Update_Order(MyDialogText.Text, MyDialogComboBox1.SelectedItem); break;
                    default: break;
                }
            }
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            MyDialogResult = ((Button)sender).Content.ToString();

            if (MyDialogResult == "OK" && MyDialog.Tag != null && MyDialog.Tag.ToString() == "GuestConfirmation")
                SubmitGuestRequest();
        }

        private void MyDialogComboBox1_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (MyDialog.Tag != null)
            {
                switch (MyDialog.Tag.ToString())
                {
                    case "HostAddHostingUnit": UpdateCityList(sender, HostHostingUnits.CitiesCollection); break;
                    case "HostUpdateHostingUnit": UpdateCityList(sender, HostHostingUnits.CitiesCollection); break;
                    default: break;
                }
            }
        }

        /// <summary>
        /// Select amenity listbox style based on window width
        /// for responsive experience when the window is resized
        /// </summary>
        private void ChooseAmenityListBoxStyle(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 870)
                gAmenities.Style = (Style)Application.Current.Resources["MyMaterialDesignListBox"];
            else
                gAmenities.Style = (Style)Application.Current.Resources["MyMaterialDesignFilterChipListBox"];
        }

        /// <summary>
        /// Programmatically change to a different tab in Main Tab Control
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
            // Home
            if (CurrentTab != Tab0 && Tab0.IsSelected)
            {
                CurrentTab = Tab0;
            }
            // Guest Request
            else if (CurrentTab != Tab1 && Tab1.IsSelected)
            {
                CurrentTab = Tab1;
                // Update count
                CountAvailableHostingUnits();
            }
            // Hosting
            else if (CurrentTab != Tab2 && Tab2.IsSelected)
            {
                CurrentTab = Tab2;
                // refresh lists in case they have changed since last tab visit
                if (HostingFrame.Content is HostRequests hostRequests)
                    hostRequests.Refresh();
                else if (HostingFrame.Content is HostOrders hostOrders)
                    hostOrders.Refresh();
                else if (HostingFrame.Content is HostHostingUnits hostHostingUnits)
                    hostHostingUnits.Refresh();
            }
            // Admin
            else if (CurrentTab != Tab3 && Tab3.IsSelected)
            {
                CurrentTab = Tab3;
                // refresh lists in case they have changed since last tab visit
                if (AdminFrame.Content is AdminRequests adminRequests)
                    adminRequests.Refresh();
                else if (AdminFrame.Content is AdminOrders adminOrders)
                    adminOrders.Refresh();
                else if (AdminFrame.Content is AdminHostingUnits adminHostingUnits)
                    adminHostingUnits.Refresh();
                else if (AdminFrame.Content is AdminHosts adminHosts)
                    adminHosts.Refresh();
            }
        }

        /// <summary>
        /// Update City List when District List changed
        /// </summary>
        private void UpdateCityList(object sender, ObservableCollection<string> cityList)
        {
            // get selected district
            if (((ComboBox)sender).SelectedItem == null) return;
            string selectedDistrict = ((ComboBox)sender).SelectedItem.ToString();
            if (!Enum.TryParse(selectedDistrict.Replace(" ", ""), out District district)) return;
            // get list of cities in district from config
            List<string> update = Config.GetCities[district].ConvertAll(c => PascalCaseToText.Convert(c));
            // clear list
            cityList.Clear();
            // add cities in district
            foreach (string item in update)
            {
                cityList.Add(item);
            }
            // if list is empty, add item that says to select a district first
            if (cityList.Count == 0)
            {
                cityList.Add("Select district first.");
            }
        }

        private void DistrictComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdateCityList(sender, DynamicCityList);
            CountAvailableHostingUnits();
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
                IList selectedAmenities = gAmenities.SelectedItems;
                SerializableDictionary<Amenity, PrefLevel> amenities = new SerializableDictionary<Amenity, PrefLevel>();
                foreach (Amenity amenity in Enum.GetValues(typeof(Amenity)))
                {
                    if (selectedAmenities.IndexOf(amenity) > -1)
                        amenities[amenity] = PrefLevel.Required;
                }

                Bl.ValidateGuestForm(fname, lname, email, entry.ToString(), release.ToString(), districtObj, cityObj, numAdults, numChildren, prefTypeObj);

                Enum.TryParse(gPrefDistrict.SelectedItem.ToString().Replace(" ", ""), out District district);
                Enum.TryParse(gPrefCity.SelectedItem.ToString().Replace(" ", ""), out City city);
                Enum.TryParse(gPrefType.SelectedItem.ToString().Replace(" ", ""), out TypeOfPlace prefType);

                Guest = new GuestRequest
                {
                    EntryDate = entry, 
                    ReleaseDate = release,
                    FirstName = fname,
                    LastName = lname,
                    Email = email,
                    PrefDistrict = district,
                    PrefCity = city,
                    PrefType = prefType,
                    NumAdults = numAdults,
                    NumChildren = numChildren,
                    PrefAmenities = amenities
                };

                if (UnitsAvailable == 0)
                    Dialog("No existing properties match your request. Do you want to submit your request anyway?", "GuestConfirmation", cancelButton: true);
                else
                    SubmitGuestRequest();
            }
            catch (Exception error)
            {
                Dialog(error.Message);
            }
        }

        private void SubmitGuestRequest()
        {
            try
            {
                Bl.CreateGuestRequest(new GuestRequest(Guest.EntryDate, Guest.ReleaseDate, Guest.FirstName, Guest.LastName, Guest.Email, Guest.PrefDistrict, Guest.PrefCity, Guest.PrefType, Guest.NumAdults, Guest.NumChildren, Guest.PrefAmenities));
                MySnackbar.MessageQueue.Enqueue("Success! Your request has been submitted.");
            }
            catch (Exception error)
            {
                MySnackbar.MessageQueue.Enqueue(error.Message);
            }
        }

        private void CountAvailableHostingUnits(object sender = null, SelectionChangedEventArgs e = null)
        {
            DateTime.TryParse(gEntryDate.SelectedDate.ToString(), out DateTime entry);
            DateTime.TryParse(gReleaseDate.SelectedDate.ToString(), out DateTime release);

            District? district = null;
            City? city = null;
            TypeOfPlace? prefType = null;
            if (gPrefDistrict.SelectedItem != null)
            {
                Enum.TryParse(gPrefDistrict.SelectedItem.ToString().Replace(" ", ""), out District d);
                district = (District?)d;
            }
            if (gPrefCity.SelectedItem != null)
            {
                Enum.TryParse(gPrefCity.SelectedItem.ToString().Replace(" ", ""), out City c);
                city = (City?)c;
            }
            if (gPrefType.SelectedItem != null)
            {
                Enum.TryParse(gPrefType.SelectedItem.ToString().Replace(" ", ""), out TypeOfPlace t);
                prefType = (TypeOfPlace?)t;
            }

            List<Amenity> amenities = (from Amenity item in gAmenities.SelectedItems select item).ToList();

            try
            {
                UnitsAvailable = Bl.GetAvailableHostingUnits(entry, release, district, city, prefType, amenities).Count();
            }
            catch
            {
                UnitsAvailable = 0;
            }

            UnitsAvailableLabel.Text = $"{UnitsAvailable} {(UnitsAvailable == 1 ? "Unit" : "Units")} Available";
            UnitsAvailableLabel.Foreground = (UnitsAvailable > 0) ? Brushes.DarkGreen : Brushes.DarkRed;
        }
    }
}
