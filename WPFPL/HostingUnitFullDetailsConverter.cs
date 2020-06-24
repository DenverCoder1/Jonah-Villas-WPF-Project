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
                        Host owner = MainWindow.Bl.GetHost(hostingUnit.OwnerHostID);
                        IEnumerable<string> amenities = from item in hostingUnit.Amenities select item.ToString();
                        StringBuilder output = new StringBuilder();
                        output.AppendLine($"Unit ID: {hostingUnit.HostingUnitKey}");
                        output.AppendLine($"Unit name: {hostingUnit.UnitName}");
                        output.AppendLine($"Location: {hostingUnit.UnitCity}, {hostingUnit.UnitDistrict}");
                        output.AppendLine($"Unit type: {hostingUnit.UnitType}");
                        output.AppendLine($"Total commissions: {hostingUnit.TotalCommissionsNIS} NIS");
                        output.AppendLine($"Amenities: {(amenities.Count() > 0 ? string.Join(", ",amenities.ToArray()) : "None")}");
                        output.AppendLine($"Owner ID: {hostingUnit.OwnerHostID}\n");
                        output.AppendLine("Reserved dates:");
                        if (hostingUnit.Calendar.Count == 0)
                            output.AppendLine("No dates have been reserved.");
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
