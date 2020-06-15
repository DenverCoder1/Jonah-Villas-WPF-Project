using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
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
                    order.GuestRequestKey == default)
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
            if (fname.Length < 2)
            {
                throw new InvalidDataException("First name must contain at least 2 letters.");
            }
            else if (!fname.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                throw new InvalidDataException("First name must only contain letters and whitespace.");
            }
            else if (lname.Length < 2)
            {
                throw new InvalidDataException("Last name must contain at least 2 letters.");
            }
            else if (!lname.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                throw new InvalidDataException("Last name must only contain letters and whitespace.");
            }
            else if (!instance.IsValidEmail(email))
            {
                throw new InvalidDataException("Email address is not valid.");
            }
            else if (!DateTime.TryParse(entryDate, out DateTime entry))
            {
                throw new InvalidDataException("Entry date is not valid.");
            }
            else if (DateTime.Compare(entry.Date, DateTime.Now.Date) < 0)
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
            else if (DateTime.Compare(release.Date, DateTime.Now.Date.AddMonths(11)) > 0)
            {
                throw new InvalidDataException("Bookings can only be made up to 11 months in advance.");
            }
            try
            {
                if (district != null && district.ToString() is string d)
                {
                    District _ = (District)Enum.Parse(typeof(District), d.Replace(" ", ""));
                }
                else
                {
                    throw new InvalidDataException("You have not selected a district.");
                }
            }
            catch (InvalidDataException e)
            {
                throw e;
            }
            catch (Exception)
            {
                throw new InvalidDataException("District selection is not valid.");
            }
            try
            {
                if (city != null && city.ToString() is string c)
                {
                    City _ = (City)Enum.Parse(typeof(City), c.Replace(" ", ""));
                }
                else
                {
                    throw new InvalidDataException("You have not selected a city.");
                }
            }
            catch (InvalidDataException e)
            {
                throw e;
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
                    TypeOfPlace _ = (TypeOfPlace)Enum.Parse(typeof(TypeOfPlace), pt.Replace(" ", ""));
                }
                else
                {
                    throw new InvalidDataException("You have not selected a rental type.");
                }
            }
            catch (InvalidDataException e)
            {
                throw e;
            }
            catch (Exception)
            {
                throw new InvalidDataException("Rental type selection is not valid.");
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
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
