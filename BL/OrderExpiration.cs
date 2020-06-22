using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BE;

namespace BL
{
    public class OrderExpiration
    {
        // file for detecting when job was run
        private static readonly string lastRunFile = @"lastRun.txt";

        /// <summary>
        /// Job that runs the expire orders function once per day
        /// </summary>
        public static void StartJob()
        {
            while (true)
            {
                // get the last time the file was written to
                DateTime lastRunTime = new System.IO.FileInfo(lastRunFile).LastWriteTime;
                // get time span since last run
                TimeSpan sinceLastRunTime = DateTime.Today - lastRunTime.Date;
                // if a day has passed
                if (sinceLastRunTime.Days >= 1)
                {
                    // write anything to the file to update the lastWriteTime
                    System.IO.File.WriteAllText(lastRunFile, DateTime.Now.ToString());
                    // Do the job
                    ExpireOrders();
                }
                else
                {
                    // Sleep for an hour
                    Thread.Sleep(1000 * 60 * 60);
                }
            }
        }

        /// <summary>
        /// Get a list of orders that are expired and mark as closed
        /// </summary>
        private static void ExpireOrders()
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
        }
    }
}
