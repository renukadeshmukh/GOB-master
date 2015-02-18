using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.DataContracts.Messages
{
    public class PlayMoveRq : Request
    {
        public string GameId { get; set; }
        public string MoveCode { get; set; }
        public int Points { get; set; }
    }
}
