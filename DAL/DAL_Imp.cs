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
            // check if guest request is already in list
            GuestRequest oldGuestRequest = instance.GetGuestRequests().Find((GuestRequest gr) =>
                gr.GuestRequestKey == guestRequest.GuestRequestKey
            );

            // if not in list
            if (oldGuestRequest == null)
            {
                // add to list
                DataSource.GuestRequests.Add(Cloning.Clone(guestRequest));
            }
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
            IEnumerable<GuestRequest> matches =
                from GuestRequest item in instance.GetGuestRequests()
                where item.GuestRequestKey == newGuestRequest.GuestRequestKey
                select item;

            // if not in list, add it to list
            if (matches.ToList().Count == 0)
                instance.CreateGuestRequest(newGuestRequest);

            GuestRequest oldGuestRequest = matches.ToList()[0];

            // find index of old guest request
            int index = DataSource.GuestRequests.FindIndex(item =>
                item.GuestRequestKey == oldGuestRequest.GuestRequestKey
            );
            
            // replace with the new guest request
            DataSource.GuestRequests[index] = Cloning.Clone(newGuestRequest);
            return true;
        }

        /// <summary>
        /// Return a deep copy of the guest request list
        /// </summary>
        List<GuestRequest> IDAL.GetGuestRequests()
        {
            return DataSource.GuestRequests.ConvertAll(x => Cloning.Clone(x));
        }

        // HOSTING UNIT

        /// <summary>
        /// Place a new hosting unit into the data
        /// </summary>
        /// <param name="hostingUnit">Hosting unit to add</param>
        /// <returns>True when successful</returns>
        bool IDAL.CreateHostingUnit(HostingUnit hostingUnit)
        {
            // check if hosting unit is already in list
            HostingUnit oldHostingUnit = instance.GetHostingUnits().Find((HostingUnit hu) =>
                hu.HostingUnitKey == hostingUnit.HostingUnitKey
            );
            // if not in list
            if (oldHostingUnit == null)
            {
                // add Hosting Unit to list
                DataSource.HostingUnits.Add(Cloning.Clone(hostingUnit));
            }
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
            HostingUnit hostingUnitToDelete = DataSource.HostingUnits.FirstOrDefault(hu =>
                hu.HostingUnitKey == hostingUnitKey
            );

            // remove
            DataSource.HostingUnits.Remove(hostingUnitToDelete);
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
            IEnumerable<HostingUnit> matches =
                from HostingUnit item in instance.GetHostingUnits()
                where item.HostingUnitKey == newHostingUnit.HostingUnitKey
                select item;

            // if not in list, add it to list
            if (matches.ToList().Count == 0)
                instance.CreateHostingUnit(newHostingUnit);

            HostingUnit oldHostingUnit = matches.ToList()[0];

            // find index of hosting unit
            int index = DataSource.HostingUnits.FindIndex(item =>
                item.HostingUnitKey == oldHostingUnit.HostingUnitKey
            );

            // replace with the new hosting unit
            DataSource.HostingUnits[index] = Cloning.Clone(newHostingUnit);
            return true;
        }

        /// <summary>
        /// Return a deep copy of the hosting units list
        /// </summary>
        List<HostingUnit> IDAL.GetHostingUnits()
        {
            return DataSource.HostingUnits.ConvertAll(x => Cloning.Clone(x));
        }

        // order

        /// <summary>
        /// Create an order in the list of orders
        /// </summary>
        /// <param name="order">Order to add</param>
        /// <returns>True if successful</returns>
        bool IDAL.CreateOrder(Order order)
        {
            // find order in list
            Order oldOrder = instance.GetOrders().FirstOrDefault((Order o) =>
                o.OrderKey == order.OrderKey
            );

            order.CreationDate = DateTime.Now.Date;

            // if not in list
            if (oldOrder == null)
                // add Order to list
                DataSource.Orders.Add(Cloning.Clone(order));

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
            IEnumerable<Order> matches =
                from Order item in instance.GetOrders()
                where item.OrderKey == newOrder.OrderKey
                select item;

            // if not in list, add it to list
            if (matches.ToList().Count == 0)
                instance.CreateOrder(newOrder);

            Order oldOrder = matches.ToList()[0];

            // find index of old order
            int index = DataSource.Orders.FindIndex(o => 
                o.OrderKey == oldOrder.OrderKey
            );

            // replace with the new order
            DataSource.Orders[index] = Cloning.Clone(newOrder);
            return true;
        }

        /// <summary>
        /// Return a deep copy of the orders list
        /// </summary>
        List<Order> IDAL.GetOrders()
        {
            return DataSource.Orders.ConvertAll(x => Cloning.Clone(x));
        }

        // hosts
        bool IDAL.CreateHost(Host host)
        {
            // check if host is already in list
            Host oldHost = instance.GetHosts().Find((Host h) =>
                h.HostKey == host.HostKey
            );

            // if not in list
            if (oldHost == null)
            {
                // add to list
                DataSource.Hosts.Add(Cloning.Clone(host));
            }
            return true;
        }
        List<Host> IDAL.GetHosts()
        {
            return DataSource.Hosts.ConvertAll(x => Cloning.Clone(x));
        }

        // bank branches

        /// <summary>
        /// Return a list of bank branches
        /// </summary>
        List<BankBranch> IDAL.GetBankBranches()
        {
            List<BankBranch> branches = new List<BankBranch> {
                new BankBranch(1,"Bank Hapoalim",11,"1 Main St.", "Tel Aviv"),
                new BankBranch(2,"Bank Leumi",21,"2 Main St.", "Tel Aviv"),
                new BankBranch(3,"Bank Mizrahi-Tefahot",31,"3 Main St.","Ramat Gan"),
                new BankBranch(4,"International Bank",41,"4 Main St.","Tel Aviv"),
                new BankBranch(5,"Discount Bank",51,"5 Main St.","Tel Aviv")
            };

            IEnumerable<BankBranch> matches =
                from BankBranch item in branches
                let bankNum = item.BankNumber
                let name = item.BankName
                let branchNum = item.BranchNumber
                let address = item.BranchAddress
                let city = item.BranchCity
                select new BankBranch {
                    BankNumber = bankNum,
                    BankName = name,
                    BranchNumber = branchNum,
                    BranchAddress = address,
                    BranchCity = city
                };

            return matches.ToList();
        }
    }
}
