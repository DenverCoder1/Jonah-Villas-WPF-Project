﻿using WPFPL;
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
using BL;
using MaterialDesignThemes.Wpf;
using BE;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace WPFPL
{
    /// <summary>
    /// Interaction logic for HostHostingUnits.xaml
    /// </summary>
    public partial class HostHostingUnits : Page
    {
        private static readonly MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        public static ObservableCollection<string> HostingUnitCollection { get; set; }

        public static ObservableCollection<string> DistrictsCollection { get; set; }

        public static ObservableCollection<string> CitiesCollection { get; set; }

        private static string Search { get; set; }

        private static int SortIndex { get; set; }

        public HostHostingUnits()
        {
            InitializeComponent();
            HostingUnitCollection = new ObservableCollection<string>();
            HostingUnits.ItemsSource = HostingUnitCollection;
            Refresh();
        }

        /// <summary>
        /// Refresh items in list and apply search and filters
        /// </summary>
        public void Refresh(object sender = null, RoutedEventArgs e = null)
        {
            if (HostingUnitCollection != null)
            {
                try
                {
                    // normalize search
                    if (Search == null) { Search = ""; }
                    string search = Normalize.Convert(Search);
                    // clear collection
                    HostingUnitCollection.Clear();
                    // list of hosting units
                    List<HostingUnit> orderedHostingUnits = new List<HostingUnit>();
                    // get hosting units and sort
                    switch (SortIndex)
                    {
                        // Oldest first
                        case -1:
                        case 0: orderedHostingUnits = MainWindow.Bl.GetHostHostingUnits(MainWindow.LoggedInHost.HostKey).OrderBy(item => item.HostingUnitKey).ToList(); break;
                        // Newest first
                        case 1: orderedHostingUnits = MainWindow.Bl.GetHostHostingUnits(MainWindow.LoggedInHost.HostKey).OrderByDescending(item => item.HostingUnitKey).ToList(); break;   
                        // Unit name A-Z
                        case 2: orderedHostingUnits = MainWindow.Bl.GetHostHostingUnits(MainWindow.LoggedInHost.HostKey).OrderBy(item => item.UnitName).ToList(); break;
                        // Unit city A-Z
                        case 3: orderedHostingUnits = MainWindow.Bl.GetHostHostingUnits(MainWindow.LoggedInHost.HostKey).OrderBy(item => item.UnitCity.ToString()).ToList(); break;
                        // Unit district A-Z
                        case 4: orderedHostingUnits = MainWindow.Bl.GetHostHostingUnits(MainWindow.LoggedInHost.HostKey).OrderBy(item => item.UnitDistrict.ToString()).ToList(); break;
                        // Newest first
                        default: orderedHostingUnits = MainWindow.Bl.GetHostHostingUnits(MainWindow.LoggedInHost.HostKey).OrderBy(item => item.HostingUnitKey).ToList(); break;
                    }
                    void registerName(string name, object scopedElement) { if (FindName(name) == null) RegisterName(name, scopedElement); }
                    MenuItem findName(string name) { return (MenuItem)FindName(name); }
                    // add items to list and filter by search
                    foreach (HostingUnit item in orderedHostingUnits)
                    {
                        // search by all public fields
                        if (Normalize.Convert(item).Contains(search))
                        {
                            // apply advanced filters
                            if (FilterMenus.FilterItemChecked(item.UnitName.ToString(), "Hosting unit name", findName, registerName, Refresh) &&
                                FilterMenus.FilterItemChecked(item.UnitDistrict.ToString(), "District", findName, registerName, Refresh) &&
                                FilterMenus.FilterItemChecked(item.UnitCity.ToString(), "City", findName, registerName, Refresh))
                            {
                                HostingUnitCollection.Add(item.ToString());
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    mainWindow.MySnackbar.MessageQueue.Enqueue(error.Message);
                }
            }
        }

        /// <summary>
        /// Button to return to host menu
        /// </summary>
        private void Return_To_Options(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostMenu());
        }

        /// <summary>
        /// Prompt for changing hosting unit
        /// </summary>
        private void Update_Hosting_Unit(object sender, RoutedEventArgs e)
        {
            if (HostingUnits.SelectedItem == null)
            {
                MainWindow.Dialog("First select a hosting unit to update.");
                return;
            }
            Match match = new Regex(@"^#(\d+) .*").Match(HostingUnits.SelectedItem.ToString());
            if (match.Success)
            {
                if (long.TryParse(match.Groups[1].Value, out long huKey))
                {
                    try
                    {
                        DistrictsCollection = new ObservableCollection<string>();
                        foreach (District district in Enum.GetValues(typeof(District)))
                        {
                            DistrictsCollection.Add(PascalCaseToText.Convert(district.ToString()));
                        }
                        var oldHostingUnit = MainWindow.Bl.GetHostingUnit(huKey);
                        District oldDistrict = oldHostingUnit.UnitDistrict;
                        City oldCity = oldHostingUnit.UnitCity;
                        var citiesInOldDistrict = Config.GetCities[oldDistrict];
                        CitiesCollection = new ObservableCollection<string>();
                        foreach (City city in citiesInOldDistrict)
                        {
                            CitiesCollection.Add(PascalCaseToText.Convert(city.ToString()));
                        }
                        string textBoxDefault = oldHostingUnit.UnitName;
                        mainWindow.MyDialogComboBox1.ItemsSource = DistrictsCollection;
                        string combo1Default = PascalCaseToText.Convert(oldDistrict.ToString());
                        mainWindow.MyDialogComboBox2.ItemsSource = CitiesCollection;
                        string combo2Default = PascalCaseToText.Convert(oldCity.ToString());
                        MainWindow.Dialog($"Enter the new name, district, and city for Hosting Unit {huKey}?", "HostUpdateHostingUnit", textBoxDefault, combo1Default, combo2Default);
                    }
                    catch (Exception error)
                    {
                        mainWindow.MySnackbar.MessageQueue.Enqueue(error.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Finish hosting unit update when prompt closed
        /// </summary>
        /// <param name="dialogText">Text from the dialog prompt</param>
        /// <param name="name">Inputted name</param>
        public void Finish_Update_Hosting_Unit(string dialogText, string name)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            if (String.IsNullOrEmpty(name) ||
                mainWindow.MyDialogComboBox1.SelectedItem == null ||
                mainWindow.MyDialogComboBox2.SelectedItem == null ||
                !Enum.TryParse(mainWindow.MyDialogComboBox1.SelectedItem.ToString().Replace(" ", ""), out District district) ||
                !Enum.TryParse(mainWindow.MyDialogComboBox2.SelectedItem.ToString().Replace(" ", ""), out City city))
            {
                mainWindow.MySnackbar.MessageQueue.Enqueue("Action was cancelled.");
                return;
            }

            Match match = new Regex(@".*Hosting Unit (\d+).*").Match(dialogText);
            if (match.Success)
            {
                if (long.TryParse(match.Groups[1].Value, out long huKey))
                {
                    try
                    {
                        HostingUnit hostingUnit = MainWindow.Bl.GetHostingUnit(huKey);

                        if (hostingUnit.UnitName != name || hostingUnit.UnitDistrict != district || hostingUnit.UnitCity != city)
                        {
                            hostingUnit.UnitName = name;
                            hostingUnit.UnitDistrict = district;
                            hostingUnit.UnitCity = city;

                            if (MainWindow.Bl.UpdateHostingUnit(hostingUnit))
                                mainWindow.MySnackbar.MessageQueue.Enqueue("Hosting unit was successfully updated.");
                        }
                        else
                        {
                            throw new Exception("Hosting unit was not changed.");
                        }
                    }
                    catch (Exception error)
                    {
                        mainWindow.MySnackbar.MessageQueue.Enqueue(error.Message.ToString());
                    }

                    Refresh();
                }
                else
                {
                    mainWindow.MySnackbar.MessageQueue.Enqueue("Action was cancelled.");
                }
            }
        }

        /// <summary>
        /// Prompt for deleting hosting unit
        /// </summary>
        private void Delete_Hosting_Unit(object sender, RoutedEventArgs e)
        {
            if (HostingUnits.SelectedItem == null)
            {
                MainWindow.Dialog("First select a hosting unit to delete.");
                return;
            }
            Match match = new Regex(@"^#(\d+) .*").Match(HostingUnits.SelectedItem.ToString());
            if (match.Success) {
                if (long.TryParse(match.Groups[1].Value, out long huKey)) {
                    MainWindow.Dialog($"Are you sure? Please check the box to confirm the deletion of Hosting Unit #{huKey}.", "HostDeleteHostingUnit", null, null, null, false);
                }
            }
        }

        /// <summary>
        /// finish deletion when prompt closed
        /// </summary>
        /// <param name="dialogText">Text from the dialog prompt</param>
        /// <param name="checkedBox">Bool whether checked box or not</param>
        public void Confirm_Delete(string dialogText, bool? checkedBox)
        {
            Match match = new Regex(@".*Unit #(\d+).*").Match(dialogText);
            if (match.Success)
            {
                if (long.TryParse(match.Groups[1].Value, out long huKey) && checkedBox != null && (bool)checkedBox)
                {
                    try
                    {
                        if (MainWindow.Bl.DeleteHostingUnit(huKey))
                        {
                            mainWindow.MySnackbar.MessageQueue.Enqueue("Success! The hosting unit was deleted.");
                            Refresh();
                        }
                    }
                    catch (Exception error)
                    {
                        mainWindow.MySnackbar.MessageQueue.Enqueue(error.Message);
                    }
                }
                else
                {
                    mainWindow.MySnackbar.MessageQueue.Enqueue("Action was cancelled.");
                }
            }
        }

        /// <summary>
        /// Prompt for adding hosting unit
        /// </summary>
        private void Add_Hosting_Unit(object sender, RoutedEventArgs e)
        {
            DistrictsCollection = new ObservableCollection<string> { "Select a district" };
            foreach (District district in Enum.GetValues(typeof(District)))
            {
                DistrictsCollection.Add(PascalCaseToText.Convert(district.ToString()));
            }
            CitiesCollection = new ObservableCollection<string> { "Select a city" };
            ObservableCollection<string> Types = new ObservableCollection<string>();
            foreach (TypeOfPlace type in Enum.GetValues(typeof(TypeOfPlace)))
            {
                Types.Add(PascalCaseToText.Convert(type.ToString()));
            }
            mainWindow.MyDialogComboBox1.ItemsSource = DistrictsCollection;
            mainWindow.MyDialogComboBox2.ItemsSource = CitiesCollection;
            mainWindow.MyDialogComboBox3.ItemsSource = Types;
            string defaultType = PascalCaseToText.Convert(TypeOfPlace.PrivateRoom.ToString());
            MainWindow.Dialog("Enter the details for the new hosting unit.", "HostAddHostingUnit", "Name", "Select a district", "Select a city", null, defaultType, true);
        }

        /// <summary>
        /// finish add unit when prompt closed
        /// </summary>
        /// <param name="name">Inputted name</param>
        public void Finish_Add_Hosting_Unit()
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            var name = mainWindow.MyDialogTextBox.Text;
            var districtSelection = mainWindow.MyDialogComboBox1.SelectedItem;
            var citySelection = mainWindow.MyDialogComboBox2.SelectedItem;
            var typeSelection = mainWindow.MyDialogComboBox3.SelectedItem;

            if (string.IsNullOrEmpty(name) ||
                districtSelection == null ||
                citySelection == null ||
                typeSelection == null ||
                !Enum.TryParse(districtSelection.ToString().Replace(" ", ""), out District district) ||
                !Enum.TryParse(citySelection.ToString().Replace(" ", ""), out City city) ||
                !Enum.TryParse(typeSelection.ToString().Replace(" ", ""), out TypeOfPlace type))
            {
                mainWindow.MySnackbar.MessageQueue.Enqueue("Action was cancelled.");
                return;
            }

            List<Amenity> amenities = (from Amenity item in mainWindow.MyDialogListBox.SelectedItems
                                       select item).ToList();

            HostingUnit hostingUnit = new HostingUnit(MainWindow.LoggedInHost, name, district, city, type, amenities);

            try
            {
                if (MainWindow.Bl.CreateHostingUnit(hostingUnit))
                    mainWindow.MySnackbar.MessageQueue.Enqueue("Hosting unit was successfully added.");
            }
            catch (Exception error)
            {
                MainWindow.Dialog(error.Message);
            }

            Refresh();
        }

        /// <summary>
        /// Button to force refresh of list
        /// </summary>
        private void Refresh_Event(object sender, RoutedEventArgs e)
        {
            Search = SearchBox.Text;
            Refresh(Search);
        }

        /// <summary>
        /// Empty search text box
        /// </summary>
        private void Clear_Search(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            Refresh();
        }

        /// <summary>
        /// On change sort method in comboBox,
        /// refresh the list
        /// </summary>
        private void Sort_Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            SortIndex = sortBy.SelectedIndex;
            Refresh();
        }
    }
}
