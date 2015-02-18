using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Core.Model
{
    public class ChatMessage
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
        public bool IsSeen { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
