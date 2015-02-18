using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.DataContracts.Messages
{
    public class PlayMoveRs : Response
    {
        public Move Move { get; set; }

        public Game Game { get; set; }

        public int TotalPoints { get; set; }

        public int Level { get; set; }
    }
}
