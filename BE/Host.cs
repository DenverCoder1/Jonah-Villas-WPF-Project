using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Host
    {
        public long HostKey { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public BankAccount BankDetails { get; set; }
        public bool BankClearance { get; set; }

        public Host()
        {
            // set host key
            HostKey = ++Config.stHostKey;
            BankClearance = true;
        }

        public Host(
            string fname,
            string lname,
            string email,
            string phone,
            BankBranch branch,
            long routing)
        {
            HostKey = ++Config.stHostKey;
            FirstName = fname;
            LastName = lname;
            Email = email;
            PhoneNumber = phone;
            BankDetails = new BankAccount(branch, routing);
            BankClearance = true;
        }

        // deep copy (clone)
        public Host Clone()
        {
            Host Clone = new Host
            {
                HostKey = this.HostKey,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                PhoneNumber = this.PhoneNumber,
                BankDetails = this.BankDetails,
                BankClearance = this.BankClearance
            };
            return Clone;
        }

        public override string ToString()
        {
            // concatenate all hosting unit info to a string
            StringBuilder output = new StringBuilder();
            output.AppendLine($"#{HostKey} : {LastName}, {FirstName}");
            output.AppendLine($"Email: {Email}, Phone: {PhoneNumber}");
            output.Append($"Bank Clearance: {BankClearance}");
            return output.ToString();
        }
    }
}
