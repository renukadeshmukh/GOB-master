using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetOnBoard.Core.Model;

namespace GetOnBoard.Data.Interfaces
{
    public interface IMovesDataProvider
    {
        List<string> GetRandomTiles(int cnt, ref List<string> gameTiles);
        List<Move> GetNextMoves(string gameId, List<string> existingMoves);
        Move SaveMove(string gameId, Move move);
        List<string> GenerateGameTiles(int totalSets);
    }
}
