using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetOnBoard.Core.Model;

namespace GetOnBoard.Data.Interfaces
{
    public interface IGameDataProvider
    {
        List<string> GetUserActiveGames(string userId);
        List<string> GetWaitingGamePlayers(List<string> userGames);
        bool StartGame(string gameId, string userId, GameStatus status, bool isActive, List<string> tiles, List<string> gameTiles);
        Game CreateGame(string userId, GameStatus status, bool isActive, List<string> tiles, List<string> gameTiles);
        List<Game> GetGameInfo(IEnumerable<string> gameIds);
        bool SaveGamePlayerStatus(string gameId, Player player);
        bool SaveGameStatus(string gameId, GameStatus status);
        bool SetGameWinner(string gameId, string playerId);
        bool SetPlayerTurn(string gameId, Player player);
        bool UpdatePlayerProfile(string playerId, int points, out int level, out int totalPoints);
        List<Account> GetTopPLayers(int count);
        List<Game> GetPreviousGames(string userId);
        Dictionary<string, List<string>> GetGameTiles(List<string> gameIds);
        bool SaveGameTiles(string gameId, List<string> gameTiles);
        List<Game> GetUserInvites();
    }
}
