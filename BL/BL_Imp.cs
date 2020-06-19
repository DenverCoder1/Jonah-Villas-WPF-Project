﻿using BE;
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
        #region Fields

        // Set up data access connection
        private DAL.IDAL DalInstance;

        // Singleton Instance
        private static IBL instance = null;

        private BackgroundWorker worker;

        #endregion

        #region Constructor

        public BL_Imp()
        {
            DalInstance = DAL.FactoryDAL.Build();
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
                IEnumerable<Order> matches = from Order item in DalInstance.GetOrders()
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
            return DalInstance.GetHostingUnits().ConvertAll(x => Cloning.Clone(x));
        }

        /// <summary>
        /// Return hosting units belonging to a given host
        /// </summary>
        /// <param name="hostKey">Key of host</param>
        /// <returns>List of host's hosting units</returns>
        List<HostingUnit> IBL.GetHostHostingUnits(long hostKey)
        {
            List<HostingUnit> hostingUnits = DalInstance.GetHostingUnits().ConvertAll(x => Cloning.Clone(x));
            IEnumerable<HostingUnit> matches = from HostingUnit item in hostingUnits
                                               where item.Owner.HostKey == hostKey
                                               select item;
            return matches.ToList();
        }

        /// <summary>
        /// Get hosting units belonging to a host which are available for the dates in the guest request
        /// </summary>
        /// <param name="hostKey">Key of host</param>
        /// <param name="guestRequest">Guest request to check</param>
        /// <returns>List of available hosting units</returns>
        List<HostingUnit> IBL.GetAvailableHostHostingUnits(long hostKey, long guestRequestKey)
        {
            List<HostingUnit> hostingUnits = DalInstance.GetHostingUnits().ConvertAll(x => Cloning.Clone(x));
            GuestRequest guestRequest = instance.GetGuestRequest(guestRequestKey);
            try
            {
                IEnumerable<HostingUnit> matches = from HostingUnit item in hostingUnits
                                                   where item.Owner.HostKey == hostKey
                                                    && item.UnitDistrict == guestRequest.PrefDistrict
                                                    && item.UnitCity == guestRequest.PrefCity
                                                    && instance.CheckOrReserveDates(item, guestRequest, false)
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
            return from HostingUnit item in instance.GetHostingUnits()
                   group item by item.UnitDistrict;
        }

        /// <summary>
        /// Get hosting units grouped by city
        /// </summary>
        IEnumerable<IGrouping<City, HostingUnit>> IBL.GetHostingUnitsByCity()
        {
            return from HostingUnit item in instance.GetHostingUnits()
                   group item by item.UnitCity;

        }

        /// <summary>
        /// Return a list of reserved date ranges for a hosting unit key
        /// </summary>
        /// <param name="huKey">hosting unit key</param>
        /// <returns>Calendar</returns>
        List<DateRange> IBL.GetDateRanges(long huKey)
        {
            HostingUnit hostingUnit = instance.GetHostingUnit(huKey);
            if (hostingUnit == null)
                throw new Exception($"Hosting unit with ID {huKey} was not found.");
            return hostingUnit.Calendar;
        }

        /// <summary>
        /// Return hosting unit given hosting unit key, returns null if not found
        /// </summary>
        HostingUnit IBL.GetHostingUnit(long huKey)
        {
            HostingUnit hostingUnit = DalInstance.GetHostingUnits().FirstOrDefault(hu => hu.HostingUnitKey == huKey);
            return hostingUnit;
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
            return DalInstance.GetGuestRequests().ConvertAll(x => Cloning.Clone(x));
        }

        /// <summary>
        /// Get guest requests matching criteria passed to function as delegate
        /// </summary>
        /// <param name="Criteria">Func delegate accepting a GuestRequest and returning a bool</param>
        /// <returns>List of matching guest requests</returns>
        List<GuestRequest> IBL.GetGuestRequests(Func<GuestRequest, bool> Criteria)
        {
            IEnumerable<GuestRequest> matches = from GuestRequest item in instance.GetGuestRequests()
                                                where Criteria(item)
                                                select item;
            return matches.ToList();
        }


        /// <summary>
        /// Get guest requests grouped by district
        /// </summary>
        IEnumerable<IGrouping<District, GuestRequest>> IBL.GetGuestRequestsByDistrict()
        {
             return from GuestRequest item in instance.GetGuestRequests()
                    group item by item.PrefDistrict;
        }

        /// <summary>
        /// Get guest requests grouped by city
        /// </summary>
        IEnumerable<IGrouping<City, GuestRequest>> IBL.GetGuestRequestsByCity()
        {
            return from GuestRequest item in instance.GetGuestRequests()
                   group item by item.PrefCity;

        }

        /// <summary>
        /// Get guest requests grouped by num people
        /// </summary>
        IEnumerable<IGrouping<int, GuestRequest>> IBL.GetGuestRequestsByPersonCount()
        {
            return from GuestRequest item in instance.GetGuestRequests()
                   group item by (item.NumAdults + item.NumChildren);
        }

        /// <summary>
        /// Get open guest requests 
        /// </summary>
        List<GuestRequest> IBL.GetOpenGuestRequests()
        {
            return instance.GetGuestRequests(delegate (GuestRequest gr) {
                return gr.Status == GuestStatus.Open || gr.Status == GuestStatus.Pending;
            });
        }

        /// <summary>
        /// Check if a guest request exists with guest request key, return request or null if not found
        /// </summary>
        GuestRequest IBL.GetGuestRequest(long grKey)
        {
            GuestRequest guestRequest = DalInstance.GetGuestRequests().FirstOrDefault(gr => gr.GuestRequestKey == grKey);
            return guestRequest;
        }

        /// <summary>
        /// Accepts a date and number of vacation days and 
        /// returns the list of all available hosting units for that range 
        /// </summary>
        List<HostingUnit> IBL.GetAvailableUnits(DateTime start, int numDays)
        {
            GuestRequest dates = new GuestRequest { EntryDate = start, ReleaseDate = start.AddDays(numDays) };
            // check date range on each hosting unit
            IEnumerable<HostingUnit> matches = from HostingUnit item in instance.GetHostingUnits()
                                               where instance.CheckOrReserveDates(item, dates, false)
                                               select item;
            return matches.ToList();
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
            if (guestRequest.EntryDate < DateTime.Today)
                throw new ArgumentException("Dates in the past cannot be reserved.");
            // requested release date is more than 11 months from now
            if (guestRequest.ReleaseDate > DateTime.Today.AddMonths(11))
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
        /// Allow data access layer to handle creation of an order
        /// </summary>
        bool IBL.CreateOrder(Order order)
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

                    Host host = instance.GetHost(hostingUnit.Owner.HostKey);

                    if (host == null)
                        throw new Exception("Could not find a host matching the hosting unit's owner ID.");

                    // Make sure the Order key is unique
                    if (DalInstance.GetOrders().Exists((Order o) => o.OrderKey == order.OrderKey))
                    {
                        throw new ArgumentException($"Order with key {order.OrderKey} already exists.");
                    }

                    // Make sure Host has not already created the order for this guest request
                    if (instance.GetOrders().Exists((Order o) =>
                        o.GuestRequestKey == order.GuestRequestKey &&
                        instance.GetHostingUnit(o.HostingUnitKey).Owner.HostKey == hostingUnit.Owner.HostKey))
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
                        throw new Exception("Cannot create order. The host does not have bank clearance.");
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


                    // Check if dates can go into the hosting unit
                    if (instance.CheckOrReserveDates(hostingUnit, guestRequest, false))
                    {
                        // if possible to reserve
                        guestRequest.Status = GuestStatus.Pending;
                        // update guest request status
                        instance.UpdateGuestRequest(guestRequest);
                        // update hosting unit with calendar changes
                        instance.UpdateHostingUnit(hostingUnit);

                        // Create order
                        order.Status = OrderStatus.SentEmail;
                        order.CreationDate = DateTime.Today;
                        order.EmailDeliveryDate = DateTime.Today;

                        // Send an email
                        //instance.SendEmail(order);

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
                        // if successfully reserved
                        // update hosting unit calendar
                        instance.UpdateHostingUnit(hostingUnit);

                        // change Guest request status
                        guestRequest.Status = GuestStatus.Complete;
                        instance.UpdateGuestRequest(guestRequest);

                        // change back status of other orders for this guest request
                        IEnumerable<Order> matches = from Order item in instance.GetOrders()
                                                     where item.GuestRequestKey == newOrder.GuestRequestKey
                                                            && item.OrderKey != newOrder.OrderKey
                                                     select item;
                        foreach (Order item in matches)
                        {
                            item.Status = OrderStatus.NotYetHandled;
                            instance.UpdateOrder(item);
                        }

                        // calculate transaction fee
                        DateRange dateRange = new DateRange(guestRequest.EntryDate, guestRequest.ReleaseDate);
                        // multiply number of accomodation nights by fee
                        float transactionFeeNIS = (dateRange.Duration - 1) * Config.TRANSACTION_FEE_NIS;

                        // TODO: Charge transaction fee to bank account
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

        /// <summary>
        /// Allow data access layer to handle retrieval of orders
        /// </summary>
        List<Order> IBL.GetOrders()
        {
            return DalInstance.GetOrders().ConvertAll(x => Cloning.Clone(x));
        }

        /// <summary>
        /// Get orders maintained by given host
        /// </summary>
        List<Order> IBL.GetHostOrders(long hostKey)
        {
            List<Order> orders = DalInstance.GetOrders().ConvertAll(x => Cloning.Clone(x));
            List<long> hostHostingUnitKeys = instance.GetHostHostingUnits(hostKey).ConvertAll(x => x.HostingUnitKey);
            IEnumerable<Order> matches = from Order item in orders
                                         where hostHostingUnitKeys.IndexOf(item.HostingUnitKey) > -1
                                         select item;
            return matches.ToList();
        }

        /// <summary>
        /// Check if an order exists with orderKey, return order or null if not found
        /// </summary>
        Order IBL.GetOrder(long orderKey)
        {
            Order order = DalInstance.GetOrders().FirstOrDefault(o => o.OrderKey == orderKey);
            return order;
        }

        /// <summary>
        /// Get a list of orders for which greater than or equal to 
        /// a given number of days have passed since the order was created
        /// </summary>
        List<Order> IBL.GetOrdersCreatedOutsideNumDays(int numDays)
        {
            IEnumerable<Order> matches = from Order item in instance.GetOrders()
                                         where instance.Duration(item.CreationDate) >= numDays
                                         select item;
            return matches.ToList();
        }

        /// <summary>
        /// Get the number of orders corresponding to a given guest request
        /// </summary>
        int IBL.GetNumOrders(GuestRequest guestRequest)
        {
            var matches = from Order item in instance.GetOrders()
                          where item.GuestRequestKey == guestRequest.GuestRequestKey
                          select new byte();
            return matches.Count();
        }

        /// <summary>
        /// Get the number of orders corresponding to a given hosting unit
        /// </summary>
        int IBL.GetNumOrders(HostingUnit hostingUnit)
        {
            var matches = from Order item in instance.GetOrders()
                          where item.HostingUnitKey == hostingUnit.HostingUnitKey
                          select new byte();
            return matches.Count();
        }

        #endregion

        #region BANK BRANCH METHODS

        List<BankBranch> IBL.GetBankBranches()
        {
            try
            {
                return DalInstance.GetBankBranches().ConvertAll(x => Cloning.Clone(x));
            }
            catch (Exception error)
            {
                throw error;
            }
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
            return DalInstance.GetHosts().ConvertAll(x => Cloning.Clone(x));
        }

        /// <summary>
        /// Get hosts grouped by num hosting units they hold
        /// </summary>
        IEnumerable<IGrouping<int, Host>> IBL.GetHostsByNumHostingUnits()
        {
            return from Host item in instance.GetHosts()
                   let numUnits = instance.GetHostHostingUnits(item.HostKey).Count()
                   group item by numUnits;
        }

        /// <summary>
        /// Check if a host exists with hostKey, return host or null if not found
        /// </summary>
        Host IBL.GetHost(long hostKey)
        {
            Host host = DalInstance.GetHosts().FirstOrDefault(h => h.HostKey == hostKey);
            return host;
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
                        instance.GetHostingUnit(o.HostingUnitKey).Owner.HostKey == newHost.HostKey))
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
                throw new InvalidDataException("Last name must be at least 2 characters long and contain only letters and whitespace.");
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
            string bankBranch,
            string routingNum)
        {
            if (!instance.IsValidName(fname))
            {
                throw new InvalidDataException("First name must be at least 2 characters long and contain only letters and whitespace.");
            }
            else if (!instance.IsValidName(lname))
            {
                throw new InvalidDataException("Last name must be at least 2 characters long and contain only letters and whitespace.");
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

        #region OTHER METHODS

        /// <summary>
        /// A function that accepts one or two dates.
        /// The function returns the number of days that have passed
        /// from the first date to the second, or if only one date
        /// has been received - from the first date to the present day
        /// </summary>
        int IBL.Duration(DateTime start, DateTime end)
        {
            if (end != default)
                return (int)(end - start).TotalDays;
            else
                return (int)(DateTime.Today - start).TotalDays;
        }

        /// <summary>
        /// Send an email in the background
        /// </summary>
        /// <param name="order">Order details</param>
        void IBL.SendEmail(Order order, RunWorkerCompletedEventHandler completed)
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(instance.Worker_DoWork);
            if (completed != null)
                worker.RunWorkerCompleted += completed;
            worker.RunWorkerAsync(order);
        }

        void IBL.Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get order details
            Order order = (Order) e.Argument;
            GuestRequest request = instance.GetGuestRequest(order.GuestRequestKey);
            HostingUnit hostingUnit = instance.GetHostingUnit(order.HostingUnitKey);
            Host host = instance.GetHost(hostingUnit.Owner.HostKey);

            // check that all exist
            if (order == null)
                throw new ArgumentException("Order cannot be null.");
            if (request == null)
                throw new ArgumentException("Guest request was not found.");
            if (hostingUnit == null)
                throw new ArgumentException("Hosting unit was not found.");
            if (host == null)
                throw new ArgumentException("Host was not found.");

            // MailMessage Create an object
            MailMessage mail = new MailMessage();

            // address of recipient (more than one can be added)
            mail.To.Add(request.Email);

            // The address from which the email was sent
            mail.From = new MailAddress(Config.FROM_EMAIL_ADDRESS);

            // message 

            mail.Subject = $"Jonah's Villas : You have a new offer from {host.FirstName}!";

            // (HTMLMessage content(Suppose the message content is in format

            StringBuilder body = new StringBuilder();
            body.AppendLine($"<h2>Jonah's Villas</h2><br/>");
            body.AppendLine($"Hi {request.FirstName},\n");
            body.Append($"{host.FirstName} {host.LastName} has an offer for you ");
            body.Append($"in {hostingUnit.UnitCity}, {hostingUnit.UnitDistrict} ");
            body.Append($"from {request.EntryDate:dd.MM.yyyy} through {request.ReleaseDate:dd.MM.yyyy}.\n\n");
            body.AppendLine($"{host.FirstName} can be contacted by email at {host.Email} or by phone at {host.PhoneNumber}.");
            body.AppendLine($"Have a great day!");
            body.AppendLine($"- Jonah from Jonah's Villas");
            mail.Body = body.ToString();

            // HTMLDefinition that the message content is in format
            mail.IsBodyHtml = true;

            // Smtp Create object type
            SmtpClient smtp = new SmtpClient
            {
                // gmailConfigure of server
                Host = "smtp.gmail.com",
                // gmailConfigure login information ( username and password ) for account e
                Credentials = new System.Net.NetworkCredential(Config.FROM_EMAIL_ADDRESS, Config.EMAIL_PASSWORD),
                // SSLBy " P. Minister requirement , an obligation to allow in this case
                EnableSsl = true
            };
            try
            {
                // Send message
                smtp.Send(mail);
            }
            catch (Exception error)
            {
                e.Result = false;
            }
            e.Result = true;
        }

        #endregion
    }
}
