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
        public MainWindow mainWindow;

        public static ObservableCollection<string> HostingUnitCollection { get; set; }

        public static ObservableCollection<string> DistrictsCollection { get; set; }

        public static ObservableCollection<string> CitiesCollection { get; set; }

        public HostHostingUnits()
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
                foreach (BE.HostingUnit item in Util.Bl.GetHostHostingUnits(Util.MyHost.HostKey))
                {
                    HostingUnitCollection.Add(item.ToString());
                }
            }
        }

        private void Return_To_Options(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostMenu());
        }

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
                    DistrictsCollection = new ObservableCollection<string>();
                    foreach (District district in Enum.GetValues(typeof(District)))
                    {
                        DistrictsCollection.Add(PascalCaseToText.Convert(district.ToString()));
                    }
                    var oldHostingUnit = Util.Bl.GetHostingUnit(huKey);
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
            }
        }

        public static void Update_Hosting_Unit_Name(string dialogText, string name)
        {
            MainWindow mainWindow = Util.GetMainWindow();

            if (String.IsNullOrEmpty(name) ||
                mainWindow.MyDialogComboBox1.SelectedItem == null ||
                mainWindow.MyDialogComboBox2.SelectedItem == null ||
                !Enum.TryParse(mainWindow.MyDialogComboBox1.SelectedItem.ToString().Replace(" ", ""), out District district) ||
                !Enum.TryParse(mainWindow.MyDialogComboBox2.SelectedItem.ToString().Replace(" ", ""), out City city))
            {
                MessageBox.Show("Action was cancelled.");
                return;
            }

            Match match = new Regex(@".*Hosting Unit (\d+).*").Match(dialogText);
            if (match.Success)
            {
                if (long.TryParse(match.Groups[1].Value, out long huKey))
                {
                    try
                    {
                        HostingUnit hostingUnit = Util.Bl.GetHostingUnit(huKey);

                        hostingUnit.UnitName = name;
                        hostingUnit.UnitDistrict = district;
                        hostingUnit.UnitCity = city;

                        Util.Bl.UpdateHostingUnit(hostingUnit);
                    }
                    catch (InvalidDataException error)
                    {
                        MainWindow.Dialog(error.Message.ToString());
                    }

                    Refresh();
                }
                else
                {
                    MessageBox.Show("Action was cancelled.");
                }
            }
        }

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
                    MainWindow.Dialog($"Are you sure? Please type out the ID of the Hosting Unit to delete it. The ID is {huKey}.", "HostDeleteHostingUnit", "", null, null);
                }
            }
        }

        public static void Confirm_Delete(string dialogText, string textBoxHuKey)
        {
            Match match = new Regex(@".*The ID is (\d+).*").Match(dialogText);
            if (match.Success)
            {
                if (long.TryParse(match.Groups[1].Value, out long huKey)
                    && long.TryParse(textBoxHuKey, out long inputtedHuKey)
                    && huKey == inputtedHuKey)
                {
                    try
                    {
                        if (Util.Bl.DeleteHostingUnit(huKey))
                        {
                            MessageBox.Show("Successfully deleted.");
                            Refresh();
                        }
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Action was cancelled.");
                }
            }
        }

        private void Add_Hosting_Unit(object sender, RoutedEventArgs e)
        {
            DistrictsCollection = new ObservableCollection<string> { "Select a district" };
            foreach (District district in Enum.GetValues(typeof(District)))
            {
                DistrictsCollection.Add(PascalCaseToText.Convert(district.ToString()));
            }
            CitiesCollection = new ObservableCollection<string> { "Select a city" };
            mainWindow.MyDialogComboBox1.ItemsSource = DistrictsCollection;
            mainWindow.MyDialogComboBox2.ItemsSource = CitiesCollection;
            MainWindow.Dialog("Enter the name, district and city of the new hosting unit.", "HostAddHostingUnit", "Name", "Select a district", "Select a city");
        }

        public static void Add_Hosting_Unit_Named(string name)
        {
            MainWindow mainWindow = Util.GetMainWindow();

            if (String.IsNullOrEmpty(name) ||
                mainWindow.MyDialogComboBox1.SelectedItem == null ||
                mainWindow.MyDialogComboBox2.SelectedItem == null ||
                !Enum.TryParse(mainWindow.MyDialogComboBox1.SelectedItem.ToString().Replace(" ", ""), out District district) ||
                !Enum.TryParse(mainWindow.MyDialogComboBox2.SelectedItem.ToString().Replace(" ", ""), out City city))
            {
                MessageBox.Show("Action was cancelled.");
                return;
            }

            HostingUnit hostingUnit = new HostingUnit(Util.MyHost, name, district, city);

            try
            {
                Util.Bl.CreateHostingUnit(hostingUnit);
            }
            catch (InvalidDataException error)
            {
                MainWindow.Dialog(error.Message.ToString());
            }

            Refresh();
        }
    }
}
