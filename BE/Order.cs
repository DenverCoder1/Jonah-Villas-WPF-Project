using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Order
    {
        public long HostingUnitKey { get; set; }
        public long GuestRequestKey { get; set; }
        public long OrderKey { get; private set; }
        public OrderStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime EmailDeliveryDate { get; set; }

        public Order()
        {
            // get next available serial number
            OrderKey = ++Config.stOrderKey;
            Status = OrderStatus.NotYetHandled;
        }

        public Order(long huKey, long grKey)
        {
            // get next available serial number
            OrderKey = ++Config.stOrderKey;
            HostingUnitKey = huKey;
            GuestRequestKey = grKey;
            Status = OrderStatus.NotYetHandled;
        }

        // deep copy (clone)
        public Order Clone()
        {
            Order Clone = new Order
            {
                HostingUnitKey = this.HostingUnitKey,
                GuestRequestKey = this.GuestRequestKey,
                OrderKey = this.OrderKey,
                Status = this.Status,
                CreationDate = this.CreationDate,
                EmailDeliveryDate = this.EmailDeliveryDate
            };
            return Clone;
        }

        public override string ToString()
        {
            // concatenate all info to a string
            StringBuilder output = new StringBuilder();
            output.AppendLine($"HostingUnitKey: {HostingUnitKey}");
            output.AppendLine($"GuestRequestKey: {GuestRequestKey}");
            output.AppendLine($"OrderKey: {OrderKey}");
            output.AppendLine($"Status: {Status}");
            output.AppendLine($"CreationDate: {CreationDate}");
            output.AppendLine($"EmailDeliveryDate: {EmailDeliveryDate}");
            return output.ToString();
        }
    }
}
