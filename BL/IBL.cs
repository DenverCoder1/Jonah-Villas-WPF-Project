using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

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

        // guest requests
        bool CreateGuestRequest(GuestRequest guestRequest);
        bool UpdateGuestRequest(GuestRequest newGuestRequest);
        List<GuestRequest> GetGuestRequests();
        List<GuestRequest> GetOpenGuestRequests();

        // orders
        bool CreateOrder(Order order);
        bool UpdateOrder(Order newOrder);
        List<Order> GetOrders();
        List<Order> GetHostOrders(long hostKey);

        // bank branches
        List<BankBranch> GetBankBranches();

        // hosts
        bool CreateHost(Host host);
        List<Host> GetHosts();
        Host GetHost(long hostKey);

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
            string bankBranch,
            string routingNum);

        bool IsValidName(string name);
        bool IsValidEmail(string email);

        bool IsValidPhoneNumber(string phone);

        bool IsValidRoutingNumber(string routing);
    }
}
