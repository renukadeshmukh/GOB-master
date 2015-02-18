using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.DataContracts.Messages
{
    public class CreateGameRq : Request
    {
        public string Method { get; set; }

        public string PlayWith { get; set; }

        public string GameId { get; set; }
    }
}
