using BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPFPL
{
    public class HostingUnitFullDetailsConverter : IValueConverter
    {
        public static readonly string noSelection = "Select a hosting unit to view its reserved date ranges.";

        /// <summary>
        /// Given string of Hosting Unit information, return list of date ranges reserved
        /// </summary>
        public static string Convert(object value)
        {
            if (value == null) return noSelection;
            string hostingUnitStr = value.ToString();
            Match match = new Regex(@"^#(\d+) :.*").Match(hostingUnitStr);
            if (match.Success)
            {
                if (long.TryParse(match.Groups[1].Value, out long huKey))
                {
                    try
                    {
                        HostingUnit hostingUnit = MainWindow.Bl.GetHostingUnit(huKey);
                        StringBuilder output = new StringBuilder();
                        output.AppendLine($"Hosting unit ID: {hostingUnit.HostingUnitKey}");
                        output.AppendLine($"Hosting unit name: {hostingUnit.UnitName}");
                        output.AppendLine($"Hosting unit location: {hostingUnit.UnitCity}, {hostingUnit.UnitDistrict}");
                        output.AppendLine($"Hosting unit total commissions: {hostingUnit.TotalCommissionsNIS} NIS");
                        output.AppendLine($"Owner ID: {hostingUnit.Owner.HostKey}\n");
                        if (hostingUnit.Calendar.Count == 0)
                            output.AppendLine("No dates have been reserved.");
                        else
                            output.AppendLine("Reserved dates:");
                        foreach (DateRange dr in hostingUnit.Calendar)
                        {
                            output.AppendLine(dr.ToString());
                        }
                        return PascalCaseToText.Convert(output.ToString());
                    }
                    catch
                    {
                        return noSelection;
                    }
                }
            }
            return noSelection;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
