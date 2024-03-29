﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Order
    {
        public long OrderKey { get; set; }
        public long HostingUnitKey { get; set; }
        public long GuestRequestKey { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime EmailDeliveryDate { get; set; }

        public Order()
        {
            Status = OrderStatus.NotYetHandled;
        }

        public Order(long huKey, long grKey)
        {
            // get next available serial number
            OrderKey = Config.NextOrderKey++;
            HostingUnitKey = huKey;
            GuestRequestKey = grKey;
            Status = OrderStatus.NotYetHandled;
        }

        public override string ToString()
        {
            // concatenate all info to a string
            StringBuilder output = new StringBuilder();
            output.AppendLine($"#{OrderKey} : Hosting Unit ID: {HostingUnitKey}, Request ID: {GuestRequestKey}");
            output.Append($"Created: {((CreationDate == default) ? "N/A" : CreationDate.ToString("dd.MM.yyyy"))}, ");
            output.Append($"Email date: {((EmailDeliveryDate == default) ? "N/A" : EmailDeliveryDate.ToString("dd.MM.yyyy"))}\n");
            output.Append($"Status: {Status}");
            return output.ToString();
        }
    }
}
