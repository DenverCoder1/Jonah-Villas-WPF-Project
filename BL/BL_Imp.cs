using BE;
using DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace BL
{
    public class BL_Imp : IBL
    {
        // SET UP DATA ACCESS CONNECTION
        public DAL.IDAL DalInstance;

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
                    throw new ApplicationException("Could not delete since the Hosting Unit has 1 or more open orders.");
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
                throw new ArgumentException("Hosting Unit cannot be null.");
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
                throw new ArgumentException("Hosting Unit cannot be null.");
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
                    order.GuestRequestKey == default)
                {
                    throw new ArgumentException("Order is missing one or more required field.");
                }

                // Make sure the Order key is unique
                if (DalInstance.GetOrders().Exists((Order o) => o.OrderKey == order.OrderKey))
                {
                    throw new ArgumentException($"Order with key {order.OrderKey} already exists.");
                }

                HostingUnit hostingUnit = instance.GetHostingUnit(order.HostingUnitKey);

                GuestRequest guestRequest = instance.GetGuestRequest(order.GuestRequestKey);

                Host host = instance.GetHost(hostingUnit.Owner.HostKey);

                if (hostingUnit == null || guestRequest == null || host == null)
                    throw new Exception("Could not find data matching the order fields.");


                if (host.BankClearance == false)
                {
                    throw new Exception("Cannot create order. The host does not have bank clearance.");
                }

                if (guestRequest.Status != GuestStatus.Open)
                {
                    throw new Exception("Request is no longer open for orders.");
                }

                if (order.Status != OrderStatus.NotYetHandled)
                {
                    throw new Exception("Orders must not be handled upon creation.");
                }

                try
                {
                    // Check if dates can go into the hosting unit
                    if (instance.CheckOrReserveDates(hostingUnit, guestRequest, false))
                    {
                        // if possible to reserve
                        guestRequest.Status = GuestStatus.Pending;
                        // update guest request status
                        instance.UpdateGuestRequest(guestRequest);
                        // update hosting unit with calendar changes
                        instance.UpdateHostingUnit(hostingUnit);

                        // TODO: send an email (next step)

                        // Create order
                        order.Status = OrderStatus.SentEmail;
                        order.CreationDate = DateTime.Today;
                        order.EmailDeliveryDate = DateTime.Today;
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
                        return true;
                    }
                    else
                    {
                        throw new Exception("The requested dates are no longer available in the Hosting Unit.");
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

        // BANK BRANCHES

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

        // HOSTS

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
        
        // VALIDATION

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

        // MISC.

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
    }
}
