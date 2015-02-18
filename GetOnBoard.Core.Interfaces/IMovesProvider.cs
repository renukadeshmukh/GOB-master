using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetOnBoard.Core.Model;

namespace GetOnBoard.Core.Interfaces
{
    public interface IMovesProvider
    {
        Move PlayUserMove(string sessionId, string gameId, Move userMove, out string errorMessage);
        Move PlayBot(UserSession session, int gameIndex);
    }
}
