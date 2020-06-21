using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using DAL;

namespace BL
{
    public interface IBL
    { 
        // hosting units
        bool CreateHostingUnit(HostingUnit hostingUnit);
        bool DeleteHostingUnit(long hostingUnitKey);
        bool UpdateHostingUnit(HostingUnit newHostingUnit);
        List<HostingUnit> GetHostingUnits();
        List<HostingUnit> GetHostHostingUnits(long hostKey);
        List<HostingUnit> GetAvailableHostHostingUnits(long hostKey, long guestRequestKey);
        IEnumerable<IGrouping<District, HostingUnit>> GetHostingUnitsByDistrict();
        IEnumerable<IGrouping<City, HostingUnit>> GetHostingUnitsByCity();
        List<DateRange> GetDateRanges(long huKey);
        HostingUnit GetHostingUnit(long huKey);

        // guest requests
        bool CreateGuestRequest(GuestRequest guestRequest);
        bool UpdateGuestRequest(GuestRequest newGuestRequest);
        List<GuestRequest> GetGuestRequests();
        List<GuestRequest> GetOpenGuestRequests();
        List<GuestRequest> GetGuestRequests(Func<GuestRequest, bool> Criteria);
        IEnumerable<IGrouping<District, GuestRequest>> GetGuestRequestsByDistrict();
        IEnumerable<IGrouping<City, GuestRequest>> GetGuestRequestsByCity();
        IEnumerable<IGrouping<int, GuestRequest>> GetGuestRequestsByPersonCount();
        GuestRequest GetGuestRequest(long grKey);
        bool CheckOrReserveDates(HostingUnit hostingUnit, GuestRequest guestRequest, bool reserve = false);
        void CancelDateRange(HostingUnit hostingUnit, DateRange dateRange);

        // orders
        bool CreateOrder(Order order, RunWorkerCompletedEventHandler RunWorkerCompleted = null);
        bool UpdateOrder(Order newOrder);
        List<Order> GetOrders();
        List<Order> GetHostOrders(long hostKey);
        Order GetOrder(long orderKey);
        List<Order> GetOrdersCreatedOutsideNumDays(int numDays);
        int GetNumOrders(GuestRequest guestRequest);
        int GetNumOrders(HostingUnit hostingUnit);

        // bank branches
        void GetBankBranches(RunWorkerCompletedEventHandler RunWorkerCompleted = null);

        // hosts
        bool CreateHost(Host host);
        bool UpdateHost(Host host);
        List<Host> GetHosts();
        IEnumerable<IGrouping<int, Host>> GetHostsByNumHostingUnits();
        Host GetHost(long hostKey);
        List<HostingUnit> GetAvailableUnits(DateTime start, int numDays);

        // validation
        bool ValidateGuestForm(
            string fname,
            string lname,
            string email,
            string entryDate,
            string releaseDate,
            object district,
            object city,
            int numAdults,
            int numChildren,
            object prefType);

        bool ValidateHostSignUp(
            string fname,
            string lname,
            string email,
            string phone,
            object bankBranch,
            string routingNum);

        bool IsValidName(string name);
        bool IsValidEmail(string email);

        bool IsValidPhoneNumber(string phone);

        bool IsValidRoutingNumber(string routing);

        // Helper Methods

        int Duration(DateTime start, DateTime end = default);
    }
}
