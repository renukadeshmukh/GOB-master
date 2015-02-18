using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GetOnBoard.Services.DataContracts.Messages
{
    public class LoginResponse : Response
    {
        public Account Account { get; set; }

        public string SessionId { get; set; }
    }
}
