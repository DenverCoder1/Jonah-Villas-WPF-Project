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
    /// Interaction logic for HostSignIn.xaml
    /// </summary>
    public partial class HostSignIn : Page
    {
        public MainWindow mainWindow;

        public static List<Control> SignInControls;
        public HostSignIn()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            SignInControls = new List<Control>{
                HostID
            };
            ShowControls();
        }

        public static void ShowControls()
        {
            Util.SetTabControlsVisibility(SignInControls, true);
        }

        public static void HideControls()
        {
            Util.SetTabControlsVisibility(SignInControls, false);
        }

        private void Host_Enter_Button_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostMenu());
        }

        private void Host_Sign_Up_Button_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostSignUp());
        }
    }
}
