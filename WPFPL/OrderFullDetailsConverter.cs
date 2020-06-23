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
    public class OrderFullDetailsConverter : IValueConverter
    {
        public static readonly string noSelection = "Select an order to view more details.";

        /// <summary>
        /// Given string of Order information, return full details
        /// </summary>
        public static string Convert(object value)
        {
            if (value == null) return noSelection;
            string orderStr = value.ToString();
            Match match = new Regex(@"^#(\d+) :.*").Match(orderStr);
            if (match.Success)
            {
                if (long.TryParse(match.Groups[1].Value, out long orderKey))
                {
                    try
                    {
                        Order order = MainWindow.Bl.GetOrder(orderKey);
                        HostingUnit hostingUnit = MainWindow.Bl.GetHostingUnit(order.HostingUnitKey);
                        GuestRequest guestRequest = MainWindow.Bl.GetGuestRequest(order.GuestRequestKey);
                        StringBuilder output = new StringBuilder();
                        output.AppendLine($"Order ID: {order.OrderKey}");
                        output.AppendLine($"Order creation date: {order.CreationDate:dd.MM.yyyy}");
                        output.AppendLine($"Email delivery date: {order.EmailDeliveryDate:dd.MM.yyyy}");
                        output.AppendLine($"Order status: {order.Status}\n");
                        output.AppendLine($"Hosting unit ID: {hostingUnit.HostingUnitKey}");
                        output.AppendLine($"Hosting unit Name: {hostingUnit.UnitName}");
                        output.AppendLine($"Hosting unit owner ID: {hostingUnit.Owner.HostKey}");
                        output.AppendLine($"Hosting unit owner: {hostingUnit.Owner.LastName}, {hostingUnit.Owner.FirstName}\n");
                        output.AppendLine($"Request ID: {guestRequest.GuestRequestKey}");
                        output.AppendLine($"Request dates: {guestRequest.EntryDate:dd.MM.yyyy} - {guestRequest.ReleaseDate:dd.MM.yyyy}");
                        output.AppendLine($"Request location: {guestRequest.PrefCity}, {guestRequest.PrefDistrict}");
                        output.Append($"Name of guest: {guestRequest.LastName}, {guestRequest.FirstName}\n");
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
