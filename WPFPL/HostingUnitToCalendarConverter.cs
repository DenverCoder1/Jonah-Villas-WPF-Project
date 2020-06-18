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
    public class HostingUnitToCalendarConverter : IValueConverter
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
                        List<DateRange> dateRanges = Util.Bl.GetDateRanges(huKey);
                        HostingUnit hostingUnit = Util.Bl.GetHostingUnit(huKey);
                        StringBuilder converted = new StringBuilder();
                        converted.AppendLine($"Hosting unit ID: {hostingUnit.HostingUnitKey}");
                        converted.AppendLine($"Hosting unit name: {hostingUnit.UnitName}");
                        converted.AppendLine($"Hosting unit location: {hostingUnit.UnitCity}, {hostingUnit.UnitDistrict}");
                        converted.AppendLine($"Owner ID: {hostingUnit.Owner.HostKey}\n");
                        if (dateRanges.Count == 0)
                            converted.AppendLine("No dates have been reserved.");
                        foreach (DateRange dr in dateRanges)
                        {
                            converted.AppendLine(dr.ToString());
                        }
                        return PascalCaseToText.Convert(converted.ToString());
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
