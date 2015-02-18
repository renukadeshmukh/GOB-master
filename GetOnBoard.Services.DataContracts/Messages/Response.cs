using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GetOnBoard.Services.DataContracts.Messages
{
    public class Response
    {
        public string ErrorMessage { get; set; }

        public bool IsSucess { get; set; }

        public int Code { get; set; }
    }
}
