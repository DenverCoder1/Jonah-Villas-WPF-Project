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
        public static IBL Bl = BL.FactoryBL.GetBL();

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
    }
}
