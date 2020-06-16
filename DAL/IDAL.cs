using BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public interface IDAL
    {
        // hosting units
        bool CreateHostingUnit(HostingUnit hostingUnit);
        bool DeleteHostingUnit(long hostingUnitKey);
        bool UpdateHostingUnit(HostingUnit newHostingUnit);
        List<HostingUnit> GetHostingUnits();

        // guest requests
        bool CreateGuestRequest(GuestRequest guestRequest);
        bool UpdateGuestRequest(GuestRequest newGuestRequest);
        List<GuestRequest> GetGuestRequests();

        // orders
        bool CreateOrder(Order order);
        bool UpdateOrder(Order newOrder);
        List<Order> GetOrders();

        // hosts
        bool CreateHost(Host host);
        List<Host> GetHosts();

        // bank branches
        List<BankBranch> GetBankBranches();
    }
}
