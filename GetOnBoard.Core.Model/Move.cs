using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Core.Model
{
    public class Move
    {
        public string Id { get; set; }

        public string Player { get; set; }

        public string MoveCode { get; set; }

        public int Points { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
