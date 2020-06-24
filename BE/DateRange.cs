using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BE
{
    [Serializable]
    public class DateRange
    {
        [XmlIgnore] public DateTime _start;
        [XmlIgnore] public DateTime _end;

        public DateTime Start
        {
            get { return _start; }
            set { _start = value; UpdateDuration(); }
        }

        public DateTime End
        {
            get { return _end; }
            set { _end = value; UpdateDuration(); }
        }

        public int Duration { get; set; }

        public DateRange()
        {
            UpdateDuration();
        }

        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
            // get duration
            UpdateDuration();
        }

        private void UpdateDuration()
        {
            if (_end != default && _start != default)
                Duration = (int)(End - Start).TotalDays;
            else
                Duration = 0;
        }

        public override string ToString()
        {
            return Start.ToString("dd.MM.yyyy") + " - " + End.ToString("dd.MM.yyyy");
        }
    }
}
