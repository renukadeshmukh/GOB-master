using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Core.Model
{
    public class UserSession
    {
        public Account Account { get; set; }

        public List<string> ActiveGames { get; set; }

        public Dictionary<string, List<string>> GameMoves { get; set; }
    }
}
