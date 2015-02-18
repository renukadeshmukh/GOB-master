using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.DataContracts.Messages
{
    public class GameStatusRs : Response
    {
        public List<Game> Games { get; set; }
    }
}
