using BE;
using DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace BL
{
    public class BL_Imp : IBL
    {
        #region Private Fields

        // Set up data access connection
        private readonly IDAL DalInstance;

        // Singleton Instance
        private static IBL instance = null;

        #endregion

        #region Constructor

        public BL_Imp()
        {
            DalInstance = DAL.FactoryDAL.GetDAL();
        }

        #endregion

        #region Singleton Method

        // Get Instance
        public static IBL GetBL()
        {
            if (instance == null)
                instance = new BL_Imp();
            return instance;
        }

        #endregion

        #region HOSTING UNIT METHODS

        /// <summary>
        /// Allow data access layer to handle adding of hosting unit
        /// </summary>
        bool IBL.CreateHostingUnit(HostingUnit hostingUnit)
        {
            if (hostingUnit == null)
            {
                throw new ArgumentNullException("Hosting unit cannot be null.");
            }
            try {
                return DalInstance.CreateHostingUnit(Cloning.Clone(hostingUnit));
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Allow data access layer to handle deletion of hosting unit
        /// </summary>
        bool IBL.DeleteHostingUnit(long hostingUnitKey)
        {
            try {
                // Check that there are no open orders in the hosting unit
                IEnumerable<Order> matches = from Order item in instance.GetOrders()
                                             let s = item.Status
                                             let stillOpen = (s == OrderStatus.NotYetHandled || s == OrderStatus.SentEmail)
                                             where item.HostingUnitKey == hostingUnitKey && stillOpen
                                             select item;

                if (matches.Count() == 0)
                    return DalInstance.DeleteHostingUnit(hostingUnitKey);
                else
                    throw new ApplicationException("Could not delete since the hosting unit has open orders.");
            }
            catch (Exception error)
            {
                throw error;
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
                return DalInstance.UpdateHostingUnit(Cloning.Clone(newHostingUnit));
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Allow data access layer to handle retrieval of hosting unit
        /// </summary>
        /// <returns>List of all hosting units</returns>
        List<HostingUnit> IBL.GetHostingUnits()
        {
            try
            {
                return DalInstance.GetHostingUnits().ConvertAll(x => Cloning.Clone(x));
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Return hosting units belonging to a given host
        /// </summary>
        /// <param name="hostKey">Key of host</param>
        /// <returns>List of host's hosting units</returns>
        List<HostingUnit> IBL.GetHostHostingUnits(long hostKey)
        {
            try
            {
                List<HostingUnit> hostingUnits = instance.GetHostingUnits().ConvertAll(x => Cloning.Clone(x));
                IEnumerable<HostingUnit> matches = from HostingUnit item in hostingUnits
                                                   where item.OwnerHostID == hostKey
                                                   select item;
                return matches.ToList();
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get hosting units belonging to a host which are available for the dates in the guest request
        /// </summary>
        /// <param name="hostKey">Key of host</param>
        /// <param name="guestRequest">Guest request to check</param>
        /// <returns>List of available hosting units</returns>
        List<HostingUnit> IBL.GetAvailableHostHostingUnits(long hostKey, long guestRequestKey)
        {
            try
            {
                List<HostingUnit> hostingUnits = instance.GetHostingUnits().ConvertAll(x => Cloning.Clone(x));
                GuestRequest guestRequest = instance.GetGuestRequest(guestRequestKey);
                IEnumerable<HostingUnit> matches = from HostingUnit item in hostingUnits
                                                   where item.OwnerHostID == hostKey
                                                    && item.UnitDistrict == guestRequest.PrefDistrict
                                                    && item.UnitCity == guestRequest.PrefCity
                                                    && instance.IsHostingUnitAvailable(item, guestRequest)
                                                   select item;
                return matches.ToList();
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get hosting units grouped by district
        /// </summary>
        IEnumerable<IGrouping<District, HostingUnit>> IBL.GetHostingUnitsByDistrict()
        {
            try
            {
                return from HostingUnit item in instance.GetHostingUnits()
                       group item by item.UnitDistrict;
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get hosting units grouped by city
        /// </summary>
        IEnumerable<IGrouping<City, HostingUnit>> IBL.GetHostingUnitsByCity()
        {
            try
            {
                return from HostingUnit item in instance.GetHostingUnits()
                       group item by item.UnitCity;
            }
            catch (Exception error)
            {
                throw error;
            }

        }

        /// <summary>
        /// Return a list of reserved date ranges for a hosting unit key
        /// </summary>
        /// <param name="huKey">hosting unit key</param>
        /// <returns>Calendar</returns>
        List<DateRange> IBL.GetDateRanges(long huKey)
        {
            try
            {
                HostingUnit hostingUnit = instance.GetHostingUnit(huKey);
                if (hostingUnit == null)
                    throw new Exception($"Hosting unit with ID {huKey} was not found.");
                return hostingUnit.Calendar;
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Return hosting unit given hosting unit key, returns null if not found
        /// </summary>
        HostingUnit IBL.GetHostingUnit(long huKey)
        {
            try
            {
                HostingUnit hostingUnit = instance.GetHostingUnits().FirstOrDefault(hu => hu.HostingUnitKey == huKey);
                return hostingUnit;
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Return whether a hosting unit is available for a guest request
        /// based on dates, location, type, and amenities
        /// </summary>
        /// <param name="hostingUnit">Hosting unit to check</param>
        /// <param name="request">Request to check</param>
        /// <returns>Boolean value of whether unit is available</returns>
        bool IBL.IsHostingUnitAvailable(HostingUnit hostingUnit, GuestRequest request)
        {
            try
            {
                var requiredAmenities = (from item in request.PrefAmenities.Keys
                                         where request.PrefAmenities[item] == PrefLevel.Required
                                         select item).ToList();
                return (hostingUnit.UnitCity == request.PrefCity) &&
                    (hostingUnit.UnitDistrict == request.PrefDistrict) &&
                    (hostingUnit.UnitType == request.PrefType) &&
                    (requiredAmenities.All(amenity => hostingUnit.Amenities.Contains(amenity))) &&
                    (instance.CheckOrReserveDates(hostingUnit, request, false));
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Return a list of hosting units that can accept request details
        /// based on dates, location, type, and amenities
        /// </summary>
        /// <returns>List of hosting units</returns>
        List<HostingUnit> IBL.GetAvailableHostingUnits(DateTime entry, DateTime release, District? prefDistrict, City? prefCity, TypeOfPlace? prefType, List<Amenity> prefAmenities)
        {
            try
            {
                var matches = from item in instance.GetHostingUnits()
                              where (prefCity == null || item.UnitCity == prefCity) &&
                                    (prefDistrict == null || item.UnitDistrict == prefDistrict) &&
                                    (prefType == null || item.UnitType == prefType) &&
                                    (prefAmenities.All(amenity => item.Amenities.Contains(amenity))) &&
                                    (entry == default || release == default || instance.CheckOrReserveDates(item, new GuestRequest { EntryDate = entry, ReleaseDate = release }, false))
                              select item;
                return matches.ToList();
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        #endregion

        #region GUEST REQUEST METHODS

        /// <summary>
        /// Allow data access layer to handle creation of a guest request
        /// </summary>
        bool IBL.CreateGuestRequest(GuestRequest guestRequest)
        {
            if (guestRequest == null)
            {
                throw new ArgumentNullException("Guest request cannot be null.");
            }
            try {
                return DalInstance.CreateGuestRequest(Cloning.Clone(guestRequest));
            }
            catch (Exception error)
            {
                throw error;
            }
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
                return DalInstance.UpdateGuestRequest(Cloning.Clone(newGuestRequest));
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Allow data access layer to handle retrieval of guest requests
        /// </summary>
        List<GuestRequest> IBL.GetGuestRequests()
        {
            try
            {
                return DalInstance.GetGuestRequests().ConvertAll(x => Cloning.Clone(x));
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get guest requests matching criteria passed to function as delegate
        /// </summary>
        /// <param name="Criteria">Func delegate accepting a GuestRequest and returning a bool</param>
        /// <returns>List of matching guest requests</returns>
        List<GuestRequest> IBL.GetGuestRequests(Func<GuestRequest, bool> Criteria)
        {
            try
            {
                IEnumerable<GuestRequest> matches = from GuestRequest item in instance.GetGuestRequests()
                                                    where Criteria(item)
                                                    select item;
                return matches.ToList();
            }
            catch (Exception error)
            {
                throw error;
            }
        }


        /// <summary>
        /// Get guest requests grouped by district
        /// </summary>
        IEnumerable<IGrouping<District, GuestRequest>> IBL.GetGuestRequestsByDistrict()
        {
            try
            {
                return from GuestRequest item in instance.GetGuestRequests()
                       group item by item.PrefDistrict;
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get guest requests grouped by city
        /// </summary>
        IEnumerable<IGrouping<City, GuestRequest>> IBL.GetGuestRequestsByCity()
        {
            try
            {
                return from GuestRequest item in instance.GetGuestRequests()
                       group item by item.PrefCity;
            }
            catch (Exception error)
            {
                throw error;
            }

        }

        /// <summary>
        /// Get guest requests grouped by num people
        /// </summary>
        IEnumerable<IGrouping<int, GuestRequest>> IBL.GetGuestRequestsByPersonCount()
        {
            try
            {
                return from GuestRequest item in instance.GetGuestRequests()
                       group item by (item.NumAdults + item.NumChildren);
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get open guest requests 
        /// </summary>
        List<GuestRequest> IBL.GetOpenGuestRequests()
        {
            try
            {
                return instance.GetGuestRequests(delegate (GuestRequest gr)
                {
                    return gr.Status == GuestStatus.Open || gr.Status == GuestStatus.Pending;
                });
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Check if a guest request exists with guest request key, return request or null if not found
        /// </summary>
        GuestRequest IBL.GetGuestRequest(long grKey)
        {
            try
            {
                GuestRequest guestRequest = instance.GetGuestRequests().FirstOrDefault(gr => gr.GuestRequestKey == grKey);
                return guestRequest;
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Accepts a date and number of vacation days and 
        /// returns the list of all available hosting units for that range 
        /// </summary>
        List<HostingUnit> IBL.GetAvailableUnits(DateTime start, int numDays)
        {
            try
            {
                GuestRequest dates = new GuestRequest { EntryDate = start, ReleaseDate = start.AddDays(numDays) };
                // check date range on each hosting unit
                IEnumerable<HostingUnit> matches = from HostingUnit item in instance.GetHostingUnits()
                                                   where instance.CheckOrReserveDates(item, dates, false)
                                                   select item;
                return matches.ToList();
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Check if guest request dates are available in a given hosting unit
        /// if reserve is set to true, dates will be reserved,
        /// if not, the function will onl return whether the dates can be reserved
        /// </summary>
        bool IBL.CheckOrReserveDates(HostingUnit hostingUnit, GuestRequest guestRequest, bool reserve)
        {
            // check if request is legal
            if (guestRequest == null)
                throw new ArgumentException("Guest request cannot be null.");
            if (hostingUnit == null)
                throw new ArgumentException("Hosting unit cannot be null.");
            // dates not set
            if (guestRequest.EntryDate == default || guestRequest.ReleaseDate == default)
                throw new ArgumentException("Guest request is missing a date.");
            // no nights reserved
            if (guestRequest.EntryDate >= guestRequest.ReleaseDate)
                throw new ArgumentException("At least one night must be reserved.");
            // request entry date is before today
            if (guestRequest.EntryDate.Date < DateTime.Today)
                throw new ArgumentException("Dates in the past cannot be reserved.");
            // requested release date is more than 11 months from now
            if (guestRequest.ReleaseDate.Date > DateTime.Today.AddMonths(11))
                throw new ArgumentException("Dates more than 11 months in the future cannot be reserved.");

            // go through calendar until reserved
            for (int i = 0; i < hostingUnit.Calendar.Count; ++i)
            {
                DateRange d = hostingUnit.Calendar[i];
                // request starts before next reserved date range
                if (guestRequest.EntryDate <= d.Start)
                {
                    // request ends before or on day of reserved date range
                    if (guestRequest.ReleaseDate <= d.Start)
                    {
                        // request starts and ends before next date range
                        // can reserve request
                        if (reserve)
                            hostingUnit.Calendar.Insert(i, new DateRange(guestRequest.EntryDate, guestRequest.ReleaseDate));
                        return true;
                    }
                    else
                    {
                        // request partially overlaps the next date range
                        // reject request
                        return false;
                    }
                }
            }

            // if requested range is after all existing date ranges (or it is the first entry)
            // can reserve request
            if (reserve)
                hostingUnit.Calendar.Add(new DateRange(guestRequest.EntryDate, guestRequest.ReleaseDate));
            return true;
        }

        void IBL.CancelDateRange(HostingUnit hostingUnit, DateRange dateRange)
        {
            // check if unit is legal
            if (hostingUnit == null)
                throw new ArgumentException("Hosting unit cannot be null.");
            // dates not set
            if (dateRange.Start == default || dateRange.End == default)
                throw new ArgumentException("Date range is missing a date.");

            // go through calendar until found
            for (int i = 0; i < hostingUnit.Calendar.Count; ++i)
            {
                // If start and end match current date range
                if (dateRange.Start == hostingUnit.Calendar[i].Start
                    && dateRange.End == hostingUnit.Calendar[i].End)
                {
                    // remove date range
                    hostingUnit.Calendar.RemoveAt(i);
                }
            }
        }

        #endregion

        #region ORDER METHODS

        /// <summary>
        /// Check order, modify statuses, reserve dates, send email
        /// Allow data access layer to handle creation of an order
        /// </summary>
        /// <param name="order">Order to add</param>
        /// <param name="RunWorkerCompleted">Function to run when email worker completes</param>
        /// <returns>True if successful</returns>
        bool IBL.CreateOrder(Order order, RunWorkerCompletedEventHandler RunWorkerCompleted)
        {
            if (order != null)
            {
                try
                {
                    // if one or more required field is missing
                    if (order.OrderKey == default ||
                    order.HostingUnitKey == default ||
                    order.GuestRequestKey == default)
                    {
                        throw new ArgumentException("Order is missing one or more required field.");
                    }

                    HostingUnit hostingUnit = instance.GetHostingUnit(order.HostingUnitKey);

                    if (hostingUnit == null)
                        throw new Exception("Could not find a hosting unit the hosting unit ID.");

                    GuestRequest guestRequest = instance.GetGuestRequest(order.GuestRequestKey);

                    if (guestRequest == null)
                        throw new Exception("Could not find a request matching the guest request ID.");

                    Host host = instance.GetHost(hostingUnit.OwnerHostID);

                    if (host == null)
                        throw new Exception("Could not find a host matching the hosting unit's owner ID.");

                    // Make sure the Order key is unique
                    if (instance.GetOrders().Exists((Order o) => o.OrderKey == order.OrderKey))
                    {
                        throw new ArgumentException($"Order with key {order.OrderKey} already exists.");
                    }

                    // Make sure Host has not already created the order for this guest request
                    if (instance.GetOrders().Exists((Order o) =>
                        o.GuestRequestKey == order.GuestRequestKey &&
                        instance.GetHostingUnit(o.HostingUnitKey).OwnerHostID == hostingUnit.OwnerHostID))
                    {
                        throw new ArgumentException($"You have already created an order for this request.");
                    }

                    // Make sure hosting unit city matches guest request city
                    if (hostingUnit.UnitDistrict != guestRequest.PrefDistrict ||
                        hostingUnit.UnitCity != guestRequest.PrefCity)
                    {
                        throw new ArgumentException($"The hosting unit's city does not match the city requested.");
                    }


                    if (host.BankClearance == false)
                    {
                        throw new Exception("Cannot create order. You do not have bank clearance.");
                    }

                    if (guestRequest.Status != GuestStatus.Open &&
                        guestRequest.Status != GuestStatus.Pending)
                    {
                        throw new Exception("Request is no longer open for orders.");
                    }

                    if (order.Status != OrderStatus.NotYetHandled)
                    {
                        throw new Exception("Orders must not be handled upon creation.");
                    }


                    // Check if all details of unit match request and dates can go in
                    if (instance.IsHostingUnitAvailable(hostingUnit, guestRequest))
                    {
                        // if possible to reserve
                        guestRequest.Status = GuestStatus.Pending;
                        // update guest request status
                        instance.UpdateGuestRequest(guestRequest);
                        // update hosting unit with calendar changes
                        instance.UpdateHostingUnit(hostingUnit);

                        // Create order
                        order.Status = OrderStatus.SentEmail;
                        order.CreationDate = DateTime.Now;
                        order.EmailDeliveryDate = DateTime.Now;

                        // Send an email
                        Mailing.StartEmailBackgroundWorker(order, RunWorkerCompleted);

                        // add order to data
                        return DalInstance.CreateOrder(Cloning.Clone(order));
                    }
                    else
                    {
                        throw new Exception("The guest request could not be added to the hosting unit.");
                    }
                }
                catch (Exception error)
                {
                    throw error;
                }
            }
            else
            {
                throw new ArgumentNullException("Order cannot be null");
            }
        }

        /// <summary>
        /// Allow data access layer to handle update of an order
        /// </summary>
        bool IBL.UpdateOrder(Order newOrder)
        {
            try
            {
                if (newOrder == null)
                    throw new ArgumentNullException("Order cannot be null.");

                Order oldOrder = instance.GetOrder(newOrder.OrderKey);

                if (oldOrder == null)
                    throw new ArgumentException("Order with this key does not yet exist.");

                // check that old status is not closed if changing status
                if (((oldOrder.Status == OrderStatus.ClosedByCustomerResponse) ||
                    (oldOrder.Status == OrderStatus.ClosedByNoCustomerResponse)) &&
                    (newOrder.Status != oldOrder.Status))
                    throw new ArgumentException("Order status can not be changed after transaction is closed.");

                // if closing order with transaction, mark off date range as reserved
                if (oldOrder.Status == OrderStatus.SentEmail &&
                    newOrder.Status == OrderStatus.ClosedByCustomerResponse)
                {
                    try
                    {
                        HostingUnit hostingUnit = instance.GetHostingUnit(newOrder.HostingUnitKey);
                        GuestRequest guestRequest = instance.GetGuestRequest(newOrder.GuestRequestKey);

                        // reserve dates in the hosting unit
                        if (instance.CheckOrReserveDates(hostingUnit, guestRequest, true))
                        {
                            // calculate transaction fee
                            DateRange dateRange = new DateRange(guestRequest.EntryDate, guestRequest.ReleaseDate);
                            // multiply number of accomodation nights by fee
                            float transactionFeeNIS = (dateRange.Duration) * Config.TRANSACTION_FEE_NIS;

                            // add transaction fee to hosting unit's charges
                            hostingUnit.TotalCommissionsNIS += transactionFeeNIS;

                            Host owner = instance.GetHost(hostingUnit.OwnerHostID);

                            // add transaction fee to host's charges
                            owner.AmountCharged += transactionFeeNIS;

                            // update host
                            instance.UpdateHost(owner);

                            // if successfully reserved
                            // update hosting unit calendar
                            instance.UpdateHostingUnit(hostingUnit);

                            // change Guest request status
                            guestRequest.Status = GuestStatus.Complete;
                            instance.UpdateGuestRequest(guestRequest);

                            // change back status of all other orders for this guest request
                            foreach (Order item in from Order item in instance.GetOrders()
                                                   where item.GuestRequestKey == newOrder.GuestRequestKey
                                                          && item.OrderKey != newOrder.OrderKey
                                                   select item)
                            {
                                item.Status = OrderStatus.Rejected;
                                instance.UpdateOrder(item);
                            }
                        }
                        else
                        {
                            throw new Exception("The requested dates are no longer available in the hosting unit.");
                        }
                    }
                    catch (Exception error)
                    {
                        throw error;
                    }
                }

                // update order in data
                try
                {
                    return DalInstance.UpdateOrder(Cloning.Clone(newOrder));
                }
                catch (Exception error)
                {
                    throw error;
                }
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Allow data access layer to handle retrieval of orders
        /// </summary>
        List<Order> IBL.GetOrders()
        {
            try
            {
                return DalInstance.GetOrders().ConvertAll(x => Cloning.Clone(x));
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get orders matching criteria passed to function as delegate
        /// </summary>
        /// <param name="Criteria">Func delegate accepting a Order and returning a bool</param>
        /// <returns>List of matching orders</returns>
        List<Order> IBL.GetOrders(Func<Order, bool> Criteria)
        {
            try
            {
                IEnumerable<Order> matches = from Order item in instance.GetOrders()
                                                    where Criteria(item)
                                                    select item;
                return matches.ToList();
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get orders maintained by given host
        /// </summary>
        List<Order> IBL.GetHostOrders(long hostKey)
        {
            try
            {
                List<Order> orders = instance.GetOrders().ConvertAll(x => Cloning.Clone(x));
                List<long> hostHostingUnitKeys = instance.GetHostHostingUnits(hostKey).ConvertAll(x => x.HostingUnitKey);
                IEnumerable<Order> matches = from Order item in orders
                                             where hostHostingUnitKeys.IndexOf(item.HostingUnitKey) > -1
                                             select item;
                return matches.ToList();
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Check if an order exists with orderKey, return order or null if not found
        /// </summary>
        Order IBL.GetOrder(long orderKey)
        {
            try
            {
                Order order = instance.GetOrders().FirstOrDefault(o => o.OrderKey == orderKey);
                return order;
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get a list of orders for which greater than or equal to 
        /// a given number of days have passed since the order was created
        /// </summary>
        List<Order> IBL.GetOrdersCreatedOutsideNumDays(int numDays)
        {
            try
            {
                IEnumerable<Order> matches = from Order item in instance.GetOrders()
                                             where instance.Duration(item.CreationDate) >= numDays
                                             select item;
                return matches.ToList();
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get the number of orders corresponding to a given guest request
        /// </summary>
        int IBL.GetNumOrders(GuestRequest guestRequest)
        {
            try
            {
                var matches = from Order item in instance.GetOrders()
                              where item.GuestRequestKey == guestRequest.GuestRequestKey
                              select new int();
                return matches.Count();
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get the number of orders corresponding to a given hosting unit
        /// </summary>
        int IBL.GetNumOrders(HostingUnit hostingUnit)
        {
            try
            {
                var matches = from Order item in instance.GetOrders()
                              where item.HostingUnitKey == hostingUnit.HostingUnitKey
                              select new int();
                return matches.Count();
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        #endregion

        #region BANK BRANCH METHODS

        void IBL.GetBankBranches(RunWorkerCompletedEventHandler RunWorkerCompleted)
        {
            FetchBanks.GetBankBranches(RunWorkerCompleted);
        }

        #endregion

        #region HOST METHODS

        /// <summary>
        /// Create host in data
        /// </summary>
        bool IBL.CreateHost(Host host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("Host cannot be null.");
            }
            try
            {
                return DalInstance.CreateHost(Cloning.Clone(host));
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get list of hosts
        /// </summary>
        /// <returns></returns>
        List<Host> IBL.GetHosts()
        {
            try
            {
                return DalInstance.GetHosts().ConvertAll(x => Cloning.Clone(x));
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Get hosts grouped by num hosting units they hold
        /// </summary>
        IEnumerable<IGrouping<int, Host>> IBL.GetHostsByNumHostingUnits()
        {
            try
            {
                return from Host item in instance.GetHosts()
                       let numUnits = instance.GetHostHostingUnits(item.HostKey).Count()
                       group item by numUnits;
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Check if a host exists with hostKey, return host or null if not found
        /// </summary>
        Host IBL.GetHost(long hostKey)
        {
            try
            {
                Host host = instance.GetHosts().FirstOrDefault(h => h.HostKey == hostKey);
                return host;
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        /// <summary>
        /// Allow data access layer to handle update of host
        /// </summary>
        bool IBL.UpdateHost(Host newHost)
        {
            if (newHost == null)
            {
                throw new ArgumentNullException("Host cannot be null.");
            }
            try
            {
                Host oldHost = instance.GetHost(newHost.HostKey);
                // Billing clearance cannot be revoked when there is an open orders
                if (oldHost.BankClearance == true && newHost.BankClearance == false)
                    if (instance.GetOrders().Exists(o =>
                        (o.Status == OrderStatus.NotYetHandled || o.Status == OrderStatus.SentEmail) &&
                        instance.GetHostingUnit(o.HostingUnitKey).OwnerHostID == newHost.HostKey))
                    {
                        throw new Exception("Billing clearance can not be revoked while there are open orders.");
                    }
                return DalInstance.UpdateHost(Cloning.Clone(newHost));
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        #endregion
        
        #region FORM VALIDATION METHODS

        /// <summary>
        /// Validate the information inputted to the guest request form
        /// </summary>
        /// <returns>True if valid, false if invalid.</returns>
        bool IBL.ValidateGuestForm(
            string fname,
            string lname,
            string email,
            string entryDate,
            string releaseDate,
            object district,
            object city,
            int numAdults,
            int numChildren,
            object prefType)
        {
            if (!instance.IsValidName(fname))
            {
                throw new InvalidDataException("First name must be at least 2 characters long and contain only letters.");
            }
            else if (!instance.IsValidName(lname))
            {
                throw new InvalidDataException("Last name must be at least 2 characters long and contain only letters.");
            }
            else if (!instance.IsValidEmail(email))
            {
                throw new InvalidDataException("Email address is not valid.");
            }
            else if (!DateTime.TryParse(entryDate, out DateTime entry))
            {
                throw new InvalidDataException("Entry date is not valid.");
            }
            else if (DateTime.Compare(entry.Date, DateTime.Today) < 0)
            {
                throw new InvalidDataException("Entry date must not be before today's date.");
            }
            else if (!DateTime.TryParse(releaseDate, out DateTime release))
            {
                throw new InvalidDataException("Departure date is not valid.");
            }
            else if (DateTime.Compare(entry.Date, release.Date) >= 0)
            {
                throw new InvalidDataException("Entry date must be before departure date.");
            }
            else if (DateTime.Compare(release.Date, DateTime.Today.AddMonths(11)) > 0)
            {
                throw new InvalidDataException("Bookings can only be made up to 11 months in advance.");
            }
            try
            {
                if (district != null && district.ToString() is string d)
                {
                    _ = (District)Enum.Parse(typeof(District), d.Replace(" ", ""));
                }
                else
                {
                    throw new InvalidDataException("You have not selected a district.");
                }
            }
            catch (InvalidDataException error)
            {
                throw error;
            }
            catch (Exception)
            {
                throw new InvalidDataException("District selection is not valid.");
            }
            try
            {
                if (city != null && city.ToString() is string c)
                {
                    _ = (City)Enum.Parse(typeof(City), c.Replace(" ", ""));
                }
                else
                {
                    throw new InvalidDataException("You have not selected a city.");
                }
            }
            catch (InvalidDataException error)
            {
                throw error;
            }
            catch (Exception)
            {
                throw new InvalidDataException("City selection is not valid.");
            }
            if (numAdults == -1)
            {
                throw new InvalidDataException("Please select the number of adults.");
            }
            else if (numChildren == -1)
            {
                throw new InvalidDataException("Please select the number of children.");
            }
            if (numAdults < 1)
            {
                throw new InvalidDataException("Booking must have at least 1 adult.");
            }
            try
            {
                if (prefType != null && prefType.ToString() is string pt)
                {
                    _ = (TypeOfPlace)Enum.Parse(typeof(TypeOfPlace), pt.Replace(" ", ""));
                }
                else
                {
                    throw new InvalidDataException("You have not selected a rental type.");
                }
            }
            catch (InvalidDataException error)
            {
                throw error;
            }
            catch (Exception)
            {
                throw new InvalidDataException("Rental type selection is not valid.");
            }
            return true;
        }

        /// <summary>
        /// Validate details from host sign up form
        /// </summary>
        bool IBL.ValidateHostSignUp(
            string fname,
            string lname,
            string email,
            string phone,
            object bankBranch,
            string routingNum)
        {
            if (!instance.IsValidName(fname))
            {
                throw new InvalidDataException("First name must be at least 2 characters long and contain only letters.");
            }
            else if (!instance.IsValidName(lname))
            {
                throw new InvalidDataException("Last name must be at least 2 characters long and contain only letters.");
            }
            else if (!instance.IsValidEmail(email))
            {
                throw new InvalidDataException("Email address is not valid.");
            }
            else if (!instance.IsValidPhoneNumber(phone))
            {
                throw new InvalidDataException("Phone number is not valid.");
            }
            else if (!instance.IsValidRoutingNumber(routingNum))
            {
                throw new InvalidDataException("Routing number is not valid.");
            }
            return true;
        }

        /// <summary>
        /// Check if an name is valid
        /// </summary>
        bool IBL.IsValidName(string name)
        {
            if (name.Length < 2)
            {
                return false;
            }
            else if (!name.All(c => char.IsLetter(c)))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if an email address is valid
        /// </summary>
        bool IBL.IsValidEmail(string email)
        {
            try
            {
                System.Net.Mail.MailAddress mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check if an phone number is valid
        /// </summary>
        bool IBL.IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return false;

            string numbersOnly = Regex.Replace(phone, @"[^0-9]+", "");

            if (numbersOnly.Length >= 7 && numbersOnly.Length <= 15)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check if an routing number is valid
        /// </summary>
        bool IBL.IsValidRoutingNumber(string routing)
        {
            if (string.IsNullOrEmpty(routing))
                return false;

            string numbersOnly = Regex.Replace(routing, @"[^0-9]+", "");
            if (numbersOnly.Length >= 8 && numbersOnly.Length <= 16)
                return long.TryParse(routing, out _);
            else
                return false;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// A function that accepts one or two dates.
        /// The function returns the number of days that have passed
        /// from the first date to the second, or if only one date
        /// has been received - from the first date to the present day
        /// </summary>
        int IBL.Duration(DateTime start, DateTime end)
        {
            if (end != default)
                return (int)(end.Date - start.Date).TotalDays;
            else
                return (int)(DateTime.Today - start.Date).TotalDays;
        }

        #endregion
    }
}
