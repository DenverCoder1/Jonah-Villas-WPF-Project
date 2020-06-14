using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Host : IEnumerable
    {
        public int HostKey { get; set; }
        public List<HostingUnit> HostingUnitCollection { get; private set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public BankAccount BankDetails { get; set; }
        public bool BankClearance { get; set; }

        public Host(int hk, int numUnits)
        {
            // set host key
            HostKey = hk;
            // create collection and fill with new hosting units
            HostingUnitCollection = new List<HostingUnit>(numUnits);
            for (int i = 0; i < numUnits; ++i)
            {
                HostingUnitCollection.Add(new HostingUnit());
            }
        }

        public override string ToString()
        {
            // concatenate all hosting unit info to a string
            StringBuilder output = new StringBuilder();
            foreach (HostingUnit hu in HostingUnitCollection)
            {
                if (hu != null)
                {
                    output.AppendLine(hu.ToString());
                }
            }
            return output.ToString();
        }

        // IEnumerator
        public IEnumerator GetEnumerator()
        {
            foreach (HostingUnit hu in HostingUnitCollection)
            {
                yield return hu;
            }
        }

        // indexer
        public HostingUnit this[int index]
        {
            get
            {
                return HostingUnitCollection[index];
            }

            set
            {
                HostingUnitCollection[index] = value;
            }
        }


    }
}
