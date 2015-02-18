using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Core.Model
{
    public class Log
    {
        private decimal _timeTaken = decimal.MinValue;

        public Log()
        {
            StartTime = DateTime.Now;
            EndTime = DateTime.Now;
            MachineName = Environment.MachineName;
        }

        public Log(string name, string request, string serviceName, int serviceId = 0)
            : this()
        {
            Name = name;
            Request = request;
            ServiceId = serviceId;
            ServiceName = serviceName;
        }

        public int LogId { get; set; }

        public string MachineName { get; set; }

        public string Name { get; set; }

        public string Request { get; set; }

        public string Response { get; set; }

        public int ServiceId { get; set; }

        public string ServiceName { get; set; }

        public string SessionId { get; set; }

        public Status Status { get; set; }

        public DateTime TimeStamp { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal TimeTaken
        {
            get
            {
                if (_timeTaken == decimal.MinValue)
                {
                    long ticks = (EndTime - StartTime).Ticks;
                    if (ticks == 0)
                        _timeTaken = 0;
                    else
                        _timeTaken = (ticks / (decimal)TimeSpan.TicksPerSecond);
                }
                return _timeTaken;
            }
            set { _timeTaken = value; }
        }

        public List<KeyValuePair<string,string>> CustomAttributes { get; set; }

        public string this[string name]
        {
            get
            {
                if (this.CustomAttributes == null) return null;
                var match = this.CustomAttributes.Find(x => string.Compare(x.Key, name, true) == 0);
                return match.Value;
            }
            set
            {
                if (this.CustomAttributes == null)
                    this.CustomAttributes = new List<KeyValuePair<string,string>>();
                else
                    this.CustomAttributes.RemoveAll(x => string.Compare(x.Key, name, true) == 0);
                this.CustomAttributes.Add(new KeyValuePair<string, string>(name,value));
            }
        }
    }

    public enum Status
    {
        Success,
        Failure
    }
}
