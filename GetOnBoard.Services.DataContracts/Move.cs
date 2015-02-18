using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Services.DataContracts
{
    public class Move
    {
        public string Id { get; set; }

        public string Player { get; set; }

        public string MoveCode { get; set; }

        public int Points { get; set; }
    }
}
