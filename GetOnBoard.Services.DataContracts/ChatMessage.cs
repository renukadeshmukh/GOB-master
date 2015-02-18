using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.DataContracts
{
    public class ChatMessage
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
        public bool IsSeen { get; set; }
        public string TimeStamp { get; set; }
    }
}
