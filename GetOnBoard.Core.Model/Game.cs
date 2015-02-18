using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Core.Model
{
    public class Game 
    {
        public string Id { get; set; }

        public string Status { get; set; }

        public string Winner { get; set; }

        public string FinishedBy { get; set; }

        public List<Player> Players { get; set; }

        public DateTime LastActivityTime { get; set; }
    }

    public enum GameStatus
    {
        Waiting,
        Invited,
        Started,
        Finished,
        Resigned,
        Cancelled
    }
}
