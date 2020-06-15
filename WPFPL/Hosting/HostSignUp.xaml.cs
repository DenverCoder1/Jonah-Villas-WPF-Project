using Project01_3693_dotNet5780;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for HostSignUp.xaml
    /// </summary>
    public partial class HostSignUp : Page
    {
        public MainWindow mainWindow;

        public static List<Control> SignUpControls;
        public HostSignUp()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            SignUpControls = new List<Control>{
                hFirstName, hLastName, hEmail, hPhone,
                hBranch, hRoutingNumber
            };
            ShowControls();
        }

        public static void ShowControls()
        {
            Util.SetTabControlsVisibility(SignUpControls, true);
        }

        public static void HideControls()
        {
            Util.SetTabControlsVisibility(SignUpControls, false);
        }

        private void Create_Account(object sender, RoutedEventArgs e)
        {
            // TODO: Create Host Account

            mainWindow.HostingFrame.Navigate(new HostMenu());
        }

        private void Return_To_Sign_In(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostSignIn());
        }
    }
}
