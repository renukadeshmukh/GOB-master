using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetOnBoard.Core.Model;

namespace GetOnBoard.Core.Interfaces
{
    public interface IGameProvider
    {
        Game CreateGame(string sessionId, string gameType, string playWith, string gameId, out string errorMessage);
        bool SetGamePlayerTurn(string gameId, string playerId, bool isActive);
        bool UpdatePlayerProfile(string playerId, int points, out int level, out int totalPoints);
        bool SetGameWinner(string gameId, string playerId);
        bool SaveGameStatus(string gameId, GameStatus status);
        bool SaveGamePlayerStatus(string gameId, Player player);
        List<string> GetGameTiles(string gameId);
        bool SetGameTiles(string gameId, List<string> gameTiles, bool save = true);
        Game GetGame(string gameId);
        List<Move> GetGameMoves(string gameid);
        List<Game> LoadGamesFromAppacitive(List<string> gameIds);
        List<string> GetInvites(string playerId);
        void SetInvite(string playerId, string gameId);
    }
}
