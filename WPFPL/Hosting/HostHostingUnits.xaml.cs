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
                    MainWindow.Dialog($"What should be the new name for Hosting Unit {huKey}?", "HostUpdateHostingUnit", true, false);
                }
            }
        }

        public static void Update_Hosting_Unit_Name(string dialogText, string name)
        {
            if (String.IsNullOrEmpty(name))
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
                    MainWindow.Dialog($"Are you sure? Please type out the ID of the Hosting Unit to delete it. The ID is {huKey}.", "HostDeleteHostingUnit", true, false);
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
            MainWindow.Dialog("What should the new Hosting Unit be called?", "HostAddHostingUnit", true, false);
        }

        public static void Add_Hosting_Unit_Named(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                MessageBox.Show("Action was cancelled.");
                return;
            }

            HostingUnit hostingUnit = new HostingUnit(Util.MyHost, name);

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
