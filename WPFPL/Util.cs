using BE;
using WPFPL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BL;

namespace WPFPL
{
    class Util
    {
        // Reference to the BL instance
        public static IBL Bl = BL_Imp.GetBL();

        // Global reference to current signed in host
        public static Host MyHost { get; set; }

        /// <summary>
        /// Get Main Window
        /// </summary>
        /// <returns>Reference to main window</returns>
        public static MainWindow GetMainWindow()
        {
            MainWindow mainWindow = null;

            foreach (Window window in Application.Current.Windows)
            {
                Type type = typeof(MainWindow);
                if (window != null && window.DependencyObjectType.Name == type.Name)
                {
                    mainWindow = (MainWindow)window;
                    if (mainWindow != null)
                    {
                        break;
                    }
                }
            }
            return mainWindow;
        }

        /// <summary>
        /// Set all input control borders from a given tab
        /// to be visible or not visible
        /// </summary>
        public static void SetTabControlsVisibility(List<Control> TabControls, bool visible)
        {
            int VISIBLE_CONTROL_WIDTH = 160;
            foreach (Control ctrl in TabControls)
            {
                ctrl.Width = visible ? VISIBLE_CONTROL_WIDTH : 0;
                ctrl.BorderThickness = visible ? new Thickness(0, 0, 0, 1) : new Thickness(0);
            }
        }
    }
}
