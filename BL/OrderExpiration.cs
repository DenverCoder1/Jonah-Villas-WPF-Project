using System;
using System.Collections.Generic;
using System.Timers;
using BE;

namespace BL
{
    public class OrderExpiration
    {
        /// <summary>
        /// Job that runs the expire orders function once per day
        /// </summary>
        public static void StartJob()
        {
            // Expire orders on first startup
            ExpireOrdersAndRequests();
            // create timer thread to run job once per day
            // set time interval to 1 day in milliseconds
            Timer timer = new Timer(TimeSpan.FromDays(1).TotalMilliseconds);
            // attach the Elapsed event for the timer
            timer.Elapsed += ExpireOrdersAndRequests;
            // start the timer
            timer.Enabled = true;
        }

        /// <summary>
        /// Get a list of orders and requests that are expired and mark as closed
        /// When the timer's Elapsed event fires it is called on a thread in the system thread-pool
        /// </summary>
        private static void ExpireOrdersAndRequests(object sender = null, ElapsedEventArgs e = null)
        {
            IBL Bl = FactoryBL.GetBL();

            // get orders that are more than the limit of days old
            List<Order> expiredOrders = Bl.GetOrdersCreatedOutsideNumDays(Config.ORDER_OPEN_LIMIT_DAYS);
            // iterate through expired orders
            foreach (Order item in expiredOrders)
            {
                if (item.Status == OrderStatus.SentEmail)
                {
                    // update status
                    item.Status = OrderStatus.ClosedByNoCustomerResponse;
                    Bl.UpdateOrder(item);
                }
            }

            // get guest requests that have entry date that have passed
            List<GuestRequest> expiredRequests = Bl.GetGuestRequests(delegate (GuestRequest gr) { return gr.EntryDate.Date < DateTime.Today; });
            // iterate through expired requests
            foreach (GuestRequest item in expiredRequests)
            {
                if (item.Status == GuestStatus.Open || item.Status == GuestStatus.Pending)
                {
                    // update status
                    item.Status = GuestStatus.Expired;
                    Bl.UpdateGuestRequest(item);
                }
            }
        }
    }
}
