using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DS
{
    // Note: This is a temporary class for this phase of the program. 
    // The lists can be initialized with input, but it is recommended
    // to initialize the lists by several values in the code, for ease of work
    public class DataSource
    {
        public static List<HostingUnit> HostingUnits = new List<HostingUnit>();
        public static List<GuestRequest> GuestRequests = new List<GuestRequest>();
        public static List<Order> Orders = new List<Order>();
        public static List<Host> Hosts = new List<Host>();
        public static List<BankBranch> BankBranches;
    }
}