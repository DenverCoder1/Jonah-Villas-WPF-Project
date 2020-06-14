using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class BankAccount
    {
        BankBranch Branch { get; set; }
        public long RoutingNumber { get; set; }

        public override string ToString()
        {
            // concatenate all info to a string
            StringBuilder output = new StringBuilder();
            output.AppendLine(Branch.ToString());
            output.AppendLine($"Routing Number: {RoutingNumber}");
            return output.ToString();
        }
    }
}
