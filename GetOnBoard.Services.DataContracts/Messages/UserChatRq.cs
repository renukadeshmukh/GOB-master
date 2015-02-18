using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.DataContracts.Messages
{
    public class UserChatRq : Request
    {
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public int FromDays { get; set; }
    }
}
