using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class DateRange
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public int Duration { get; private set; }

        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
            // get duration
            Duration = (int)(End - Start).TotalDays;
        }

        public override string ToString()
        {
            return Start.ToString("dd.MM.yyyy") + " - " + End.ToString("dd.MM.yyyy");
        }
    }
}
