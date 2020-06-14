using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using DS;

namespace DAL
{
    // Create, Read, Update, Delete on Data Source with list implementation
    public class DAL_Imp : IDAL
    {        
        // Singleton instance
        private static IDAL instance = null;

        // Get instance
        public static IDAL GetDAL()
        {
            if (instance == null)
                instance = new DAL_Imp();
            return instance;
        }

        // GUEST REQUEST

        /// <summary>
        /// Place a new Guest Request into the data
        /// </summary>
        /// <param name="guestRequest">Guest request to add</param>
        /// <returns>True if successful</returns>
        bool IDAL.CreateGuestRequest(GuestRequest guestRequest)
        {
            // add to list
            DataSource.GuestRequests.Add(guestRequest.Clone());
            return true;
        }

        /// <summary>
        /// Update Guest request in the data
        /// </summary>
        /// <param name="newGuestRequest">Updated Guest request</param>
        /// <returns>True if successful</returns>
        bool IDAL.UpdateGuestRequest(GuestRequest newGuestRequest)
        {
            // find guest request in list
            GuestRequest oldGuestRequest = instance.GetGuestRequests().Find((GuestRequest gr) =>
                gr.GuestRequestKey == newGuestRequest.GuestRequestKey
            );

            if (oldGuestRequest == null)
                throw new ArgumentException("No Guest Request was found with this key.");

            // find index of old guest request
            int index = DataSource.GuestRequests.IndexOf(oldGuestRequest);
            
            // replace with the new guest request
            DataSource.GuestRequests[index] = newGuestRequest.Clone();
            return true;
        }

        /// <summary>
        /// Return a deep copy of the guest request list
        /// </summary>
        List<GuestRequest> IDAL.GetGuestRequests()
        {
            return DataSource.GuestRequests.ConvertAll(x => x.Clone());
        }

        // HOSTING UNIT

        /// <summary>
        /// Place a new hosting unit into the data
        /// </summary>
        /// <param name="hostingUnit">Hosting unit to add</param>
        /// <returns>True when successful</returns>
        bool IDAL.CreateHostingUnit(HostingUnit hostingUnit)
        {
            // add Hosting Unit to list
            DataSource.HostingUnits.Add(hostingUnit.Clone());
            return true;
        }
        /// <summary>
        /// Remove Hosting Unit from data given key
        /// </summary>
        /// <param name="hostingUnitKey">Key of Hosting unit</param>
        /// <returns>True when successful</returns>
        bool IDAL.DeleteHostingUnit(long hostingUnitKey)
        {
            // find hosting unit in list
            HostingUnit oldHostingUnit = instance.GetHostingUnits().Find((HostingUnit hu) =>
                hu.HostingUnitKey == hostingUnitKey
            );

            if (oldHostingUnit == null)
                throw new ArgumentException("No Hosting Unit was found with this key.");

            // find index of old hosting unit
            int index = DataSource.HostingUnits.IndexOf(oldHostingUnit);

            // remove
            DataSource.HostingUnits.Remove(oldHostingUnit);
            return true;
        }

        /// <summary>
        /// Update Hosting unit in the data
        /// </summary>
        /// <param name="newHostingUnit">Updated Hosting unit</param>
        /// <returns>True if successful</returns>
        bool IDAL.UpdateHostingUnit(HostingUnit newHostingUnit)
        {
            // find hosting unit in list
            HostingUnit oldHostingUnit = instance.GetHostingUnits().Find((HostingUnit hu) =>
                hu.HostingUnitKey == newHostingUnit.HostingUnitKey
            );

            if (oldHostingUnit == null)
                throw new ArgumentException("No Hosting Unit was found with this key.");

            // find index of old hosting unit
            int index = DataSource.HostingUnits.IndexOf(oldHostingUnit);

            // replace with the new hosting unit
            DataSource.HostingUnits[index] = newHostingUnit.Clone();
            return true;
        }

        /// <summary>
        /// Return a deep copy of the hosting units list
        /// </summary>
        List<HostingUnit> IDAL.GetHostingUnits()
        {
            return DataSource.HostingUnits.ConvertAll(x => x.Clone());
        }

        // order

        /// <summary>
        /// Create an order in the list of orders
        /// </summary>
        /// <param name="order">Order to add</param>
        /// <returns>True if successful</returns>
        bool IDAL.CreateOrder(Order order)
        {
            // add Order to list
            DataSource.Orders.Add(order.Clone());
            return true;
        }

        /// <summary>
        /// Update order in the data
        /// </summary>
        /// <param name="newOrder">Updated order</param>
        /// <returns>True if successful</returns>
        bool IDAL.UpdateOrder(Order newOrder)
        {
            // find order in list
            Order oldOrder = instance.GetOrders().Find((Order o) =>
                o.OrderKey == newOrder.OrderKey
            );

            if (oldOrder == null)
                throw new ArgumentException("No order was found with this key.");

            // find index of old order
            int index = DataSource.Orders.IndexOf(oldOrder);

            // replace with the new order
            DataSource.Orders[index] = newOrder.Clone();
            return true;
        }

        /// <summary>
        /// Return a deep copy of the orders list
        /// </summary>
        List<Order> IDAL.GetOrders()
        {
            return DataSource.Orders.ConvertAll(x => x.Clone());
        }
        
        // bank branches

        /// <summary>
        /// Return a list of bank branches
        /// </summary>
        List<BankBranch> IDAL.GetBankBranches()
        {
            return new List<BankBranch> {
                new BankBranch(1,"Bank Hapoalim",11,"1 Main St.", "Tel Aviv"),
                new BankBranch(2,"Bank Leumi",21,"2 Main St.", "Tel Aviv"),
                new BankBranch(3,"Bank Mizrahi-Tefahot",31,"3 Main St.","Ramat Gan"),
                new BankBranch(4,"International Bank",41,"4 Main St.","Tel Aviv"),
                new BankBranch(5,"Discount Bank",51,"5 Main St.","Tel Aviv")
            };
        }
    }
}
