using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    [Serializable]
    public class BankBranch
    {
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string BranchCode { get; set; }
        public string BranchAddress { get; set; }
        public string BranchCity { get; set; }

        public BankBranch() { }

        public BankBranch(string bankCode, string name, string branchCode, string address, string city)
        {
            BankCode = bankCode;
            BankName = name;
            BranchCode = branchCode;
            BranchAddress = address;
            BranchCity = city;
        }

        public string GetAllDetails()
        {
            // concatenate all info to a string
            StringBuilder output = new StringBuilder();
            output.AppendLine($"Bank Number: {BankCode}");
            output.AppendLine($"Bank Name: {BankName}");
            output.AppendLine($"Branch Number: {BranchCode}");
            output.AppendLine($"Branch Address: {BranchAddress}");
            output.AppendLine($"Branch City: {BranchCity}");
            return output.ToString();
        }

        public override string ToString()
        {
            return $"{BankName} Branch {BranchCode}";
        }
    }
}
