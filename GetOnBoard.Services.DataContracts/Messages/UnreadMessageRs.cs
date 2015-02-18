using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.DataContracts.Messages
{
    public class UnreadMessageRs : Response
    {
        public List<UnReadMessageCount> UnReadMessages { get; set; }
    }

    public class UnReadMessageCount
    {
        public string UserName { get; set; }
        public int Count { get; set; }
    }
}
