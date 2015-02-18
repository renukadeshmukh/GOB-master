using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.DataContracts.Messages
{
    public class GetGameMovesRs : Response
    {
        public string GameId { get; set; }
        public List<Move> Moves { get; set; }
        public bool IsMyTurn { get; set; }
        public Game Game { get; set; }
    }
}
