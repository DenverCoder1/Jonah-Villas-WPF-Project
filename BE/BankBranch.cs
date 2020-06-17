using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class BankBranch
    {
        public long BankNumber { get; set; }
        public string BankName { get; set; }
        public long BranchNumber { get; set; }
        public string BranchAddress { get; set; }
        public string BranchCity { get; set; }

        public BankBranch() { }

        public BankBranch(long bankNum, string name, long branchNum, string address, string city)
        {
            BankNumber = bankNum;
            BankName = name;
            BranchNumber = branchNum;
            BranchAddress = address;
            BranchCity = city;
        }

        public string GetAllDetails()
        {
            // concatenate all info to a string
            StringBuilder output = new StringBuilder();
            output.AppendLine($"Bank Number: {BankNumber}");
            output.AppendLine($"Bank Name: {BankName}");
            output.AppendLine($"Branch Number: {BranchNumber}");
            output.AppendLine($"Branch Address: {BranchAddress}");
            output.AppendLine($"Branch City: {BranchCity}");
            return output.ToString();
        }

        public override string ToString()
        {
            return $"{BankName} Branch {BranchNumber}";
        }
    }
}
