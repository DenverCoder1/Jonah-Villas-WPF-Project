using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BE;

namespace BL
{
    public class BL_Imp : IBL
    {
        // SET UP DATA ACCESS CONNECTION
        private DAL.IDAL DalInstance;

        public BL_Imp()
        {
            DalInstance = DAL.FactoryDAL.Build();
        }

        // Singleton Instance
        private static IBL instance = null;

        // Get Instance
        public static IBL GetBL()
        {
            if (instance == null)
                instance = new BL_Imp();
            return instance;
        }

        // HOSTING UNIT

        /// <summary>
        /// Allow data access layer to handle adding of hosting unit
        /// </summary>
        bool IBL.CreateHostingUnit(HostingUnit hostingUnit)
        {
            if (hostingUnit == null)
            {
                throw new ArgumentNullException("Hosting unit cannot be null.");
            }
            // Make sure the Hosting Unit is unique
            if (DalInstance.GetHostingUnits().Exists((HostingUnit hu) => hu.HostingUnitKey == hostingUnit.HostingUnitKey))
            {
                throw new ArgumentException($"Hosting Unit with key {hostingUnit.HostingUnitKey} already exists.");
            }
            return DalInstance.CreateHostingUnit(hostingUnit.Clone());
        }

        /// <summary>
        /// Allow data access layer to handle deletion of hosting unit
        /// </summary>
        bool IBL.DeleteHostingUnit(long hostingUnitKey)
        {
            try { 
                return DalInstance.DeleteHostingUnit(hostingUnitKey); 
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Allow data access layer to handle update of hosting unit
        /// </summary>
        bool IBL.UpdateHostingUnit(HostingUnit newHostingUnit)
        {
            if (newHostingUnit == null)
            {
                throw new ArgumentNullException("Hosting unit cannot be null.");
            }
            try
            {
                return DalInstance.UpdateHostingUnit(newHostingUnit.Clone());
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Allow data access layer to handle retrieval of hosting unit
        /// </summary>
        List<HostingUnit> IBL.GetHostingUnits()
        {
            return DalInstance.GetHostingUnits().ConvertAll(x => x.Clone());
        }

        // check if date range can be reserved, reserve date range in unit
        public GuestStatus ApproveRequest(object currHostingUnit, object currGuestReq)
        {
            // check if request is legal
            if (currGuestReq == null)
            {
                throw new ArgumentException("Guest request cannot be null.");
            }
            if (currHostingUnit == null)
            {
                throw new ArgumentException("Guest request cannot be null.");
            }

            // make sure obj as a Guest Request is not null
            if (currGuestReq is GuestRequest guestReq && currHostingUnit is HostingUnit hostingUnit)
            {
                // dates not set
                if (guestReq.EntryDate == default || guestReq.ReleaseDate == default)
                {
                    guestReq.Status = GuestStatus.Rejected;
                    throw new ArgumentException("Guest request is missing a date.");
                }
                // no nights reserved
                if (guestReq.EntryDate >= guestReq.ReleaseDate)
                {
                    guestReq.Status = GuestStatus.Rejected;
                    throw new ArgumentException("At least one night must be reserved.");
                }
                // request entry date is before today
                if (guestReq.EntryDate < DateTime.Now.Date)
                {
                    guestReq.Status = GuestStatus.Rejected;
                    throw new ArgumentException("The requested date range has passed.");
                }
                // requested release date is more than 11 months from now
                if (guestReq.ReleaseDate > DateTime.Now.Date.AddMonths(11))
                {
                    guestReq.Status = GuestStatus.Rejected;
                    throw new ArgumentException("Dates in the past cannot be reserved.");
                }

                // go through Diary until reserved
                for (int i = 0; i < hostingUnit.Calendar.Count; ++i)
                {
                    DateRange d = hostingUnit.Calendar[i];
                    // request starts before next reserved date range
                    if (guestReq.EntryDate < d.Start)
                    {
                        // request ends before or on day of reserved date range
                        if (guestReq.ReleaseDate <= d.Start)
                        {
                            // request starts and ends before next date range
                            // reserve request
                            hostingUnit.Calendar.Insert(i, new DateRange(guestReq.EntryDate, guestReq.ReleaseDate));
                            guestReq.Status = GuestStatus.Pending;
                            return guestReq.Status;
                        }
                        else
                        {
                            // request partially overlaps the next date range
                            // reject request
                            guestReq.Status = GuestStatus.Rejected;
                            return guestReq.Status;
                        }
                    }
                }

                // if requested range is after all existing date ranges (or it is the first entry)
                // reserve request
                hostingUnit.Calendar.Add(new DateRange(guestReq.EntryDate, guestReq.ReleaseDate));
                guestReq.Status = GuestStatus.Pending;
                return guestReq.Status;
            }

            // default
            return GuestStatus.Rejected;
        }

        // Get the number of occupied days
        public int GetAnnualBusyDays(object hu)
        {
            if (hu != null && hu is HostingUnit hostingUnit)
            {
                int count = 0;
                foreach (DateRange d in hostingUnit.Calendar)
                {
                    // if bool is true, add 1
                    if (d != null)
                    {
                        // add duration of each range (not including departure dates)
                        count += (d.Duration - 1);
                    }
                }
                // return count
                return count;
            }
            throw new ArgumentException("Object passed is not a hosting unit.");
        }

        // Get occupancy percentage
        public float GetAnnualBusyPercentage(object hu)
        {
            if (hu != null && hu is HostingUnit hostingUnit)
            {
                float count = (float)GetAnnualBusyDays(hostingUnit);
                // using 365 as the length of a year since most years contain 365 days
                return count / 365;
            }
            throw new ArgumentException("Object passed is not a hosting unit.");
        }

        // HOST

        private long SubmitRequest(Host h, GuestRequest guestReq)
        {
            long huKey = -1;
            foreach (HostingUnit hu in h.HostingUnitCollection)
            {
                try
                {
                    // if approved, get the hosting unit key
                    if (hu != null && ApproveRequest(hu, guestReq) != GuestStatus.Rejected)
                    {
                        huKey = hu.HostingUnitKey;
                        break;
                    }
                }
                catch (ArgumentException)
                {
                    continue;
                }
            }
            return huKey; // -1 if no hosting unit found
        }

        public int GetHostAnnualBusyDays(Host h)
        {
            // sum busy day counts of all hosting units
            int busyDays = 0;
            foreach (HostingUnit hu in h.HostingUnitCollection)
            {
                if (hu != null)
                {
                    busyDays += GetAnnualBusyDays(hu);
                }
            }
            return busyDays;
        }

        public void SortUnits(Host h)
        {
            // sort collection
            if (h.HostingUnitCollection != null && h.HostingUnitCollection.Count >= 1)
            {
                h.HostingUnitCollection.Sort();
            }
        }

        public bool AssignRequests(Host h, params object[] args)
        {
            bool success = true; // no problems found
            foreach (object obj in args)
            {
                if (obj != null)
                {
                    // make sure obj is a GuestRequest and not null
                    if (obj is GuestRequest guestReq)
                    {
                        // submit request and check if successful
                        if (SubmitRequest(h, guestReq) == -1)
                        {
                            // no hosting unit available
                            success = false;
                        }
                    }
                    else
                    {
                        // object is not a GuestRequest
                        throw new ArgumentException("Argument is not a GuestRequest.");
                    }
                }
            }
            // true if no problems, false if 1+ request failed
            return success;
        }

        // GUEST REQUESTS

        /// <summary>
        /// Allow data access layer to handle creation of a guest request
        /// </summary>
        bool IBL.CreateGuestRequest(GuestRequest guestRequest)
        {
            if (guestRequest == null)
            {
                throw new ArgumentNullException("Guest request cannot be null.");
            }
            // Make sure the Guest Request is unique
            if (DalInstance.GetGuestRequests().Exists((GuestRequest gr) => gr.GuestRequestKey == guestRequest.GuestRequestKey))
            {
                throw new ArgumentException($"Guest Request with key {guestRequest.GuestRequestKey} already exists.");
            }
            return DalInstance.CreateGuestRequest(guestRequest.Clone());
        }

        /// <summary>
        /// Allow data access layer to handle update of a guest request (status)
        /// </summary>
        bool IBL.UpdateGuestRequest(GuestRequest newGuestRequest)
        {
            if (newGuestRequest == null)
            {
                throw new ArgumentNullException("Guest request cannot be null.");
            }
            try
            {
                return DalInstance.UpdateGuestRequest(newGuestRequest.Clone());
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Allow data access layer to handle retrieval of guest requests
        /// </summary>
        List<GuestRequest> IBL.GetGuestRequests()
        {
            return DalInstance.GetGuestRequests().ConvertAll(x => x.Clone());
        }

        // ORDER

        /// <summary>
        /// Allow data access layer to handle creation of an order
        /// </summary>
        bool IBL.CreateOrder(Order order)
        {
            if (order != null)
            {
                // if one or more required field is missing
                if (order.OrderKey == default ||
                    order.HostingUnitKey == default ||
                    order.GuestRequestKey == default ||
                    order.CreationDate == default ||
                    order.Status == default)
                {
                    throw new ArgumentException("Order is missing one or more required field.");
                }

                // Make sure the Order key is unique
                if (DalInstance.GetOrders().Exists((Order o) => o.OrderKey == order.OrderKey))
                {
                    throw new ArgumentException($"Order with key {order.OrderKey} already exists.");
                }
            }
            else
            {
                throw new ArgumentNullException("Order cannot be null");
            }
            return DalInstance.CreateOrder(order.Clone());
        }

        /// <summary>
        /// Allow data access layer to handle update of an order
        /// </summary>
        bool IBL.UpdateOrder(Order newOrder)
        {
            if (newOrder == null)
            {
                throw new ArgumentNullException("Order cannot be null.");
            }
            try
            {
                return DalInstance.UpdateOrder(newOrder.Clone());
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Allow data access layer to handle retrieval of orders
        /// </summary>
        List<Order> IBL.GetOrders()
        {
            return DalInstance.GetOrders().ConvertAll(x => x.Clone());
        }

        // BANK BRANCHES

        List<BankBranch> IBL.GetBankBranches()
        {
            return DalInstance.GetBankBranches().ConvertAll(x => x.Clone());
        }
    }
}
