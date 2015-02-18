using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetOnBoard.Core.Interfaces;
using GetOnBoard.Core.Model;
using GetOnBoard.Data.Interfaces;

namespace GetOnBoard.Core.GameMgmt
{
    public class GameProvider : IGameProvider
    {
        private readonly ICacheProvider _cacheProvider = null;
        private readonly IGameDataProvider _gameDataProvider = null;
        private readonly IMovesDataProvider _movesDataProvider = null;
        private static object _lockObj = new object();
        private static object _lockInvites = new object();

        public GameProvider(ICacheProvider cacheProvider, IGameDataProvider gameDataProvider, IMovesDataProvider movesDataProvider)
        {
            _cacheProvider = cacheProvider;
            _gameDataProvider = gameDataProvider;
            _movesDataProvider = movesDataProvider;
        }

        public Game CreateGame(string sessionId, string gameType, string playWith, string gameId, out string errorMessage)
        {
            errorMessage = string.Empty;
            var session = _cacheProvider.GetValue(sessionId) as UserSession;
            if (session == null)
            {
                errorMessage = "Invalid session! Login again.";
                return null;
            }
            if (session.ActiveGames == null)
            {
                session.ActiveGames = _gameDataProvider.GetUserActiveGames(session.Account.Id);
                LoadGamesFromAppacitive(session.ActiveGames);
            }
            var gameCount = 0;
            foreach (var activeGameId in session.ActiveGames)
            {
                var g = GetGame(activeGameId);
                if (string.Equals(g.Status,GameStatus.Started.ToString(), StringComparison.OrdinalIgnoreCase) || 
                    string.Equals(g.Status,GameStatus.Invited.ToString(), StringComparison.OrdinalIgnoreCase) || 
                    string.Equals(g.Status,GameStatus.Waiting.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    gameCount++;
                }
            }
            if (gameCount >= session.Account.MaxGameLimit)
            {
                errorMessage = string.Format("You are allowed to play only {0} number of games at the same time!", session.Account.MaxGameLimit);
                return null;
            }
            Game game = null;
            List<string> gameTiles = _movesDataProvider.GenerateGameTiles(AppConfig.TileSetCountPerGame);
            if (string.Equals(gameType, "Random", StringComparison.CurrentCultureIgnoreCase))
            {
                game = GetRandomGame(session, ref gameTiles);
            }
            if (string.Equals(gameType, "Bot", StringComparison.CurrentCultureIgnoreCase))
            {
                game = GetBotGame(session);
            }
            if (string.Equals(gameType, "PlayWith", StringComparison.CurrentCultureIgnoreCase))
            {
                game = PlayWith(session, playWith, gameId, ref gameTiles);
            }
            SetGameTiles(game.Id, gameTiles);
            session.ActiveGames.Add(game.Id);
            game = GetGame(game.Id);
            return game;
        }
        
        public bool SetGamePlayerTurn(string gameId, string playerId, bool isActive)
        {
            var game = GetGame(gameId);
            if (game != null)
            {
                var playerIndex = game.Players.FindIndex(p => string.Equals(p.Id, playerId));
                game.Players[playerIndex].IsActive = isActive;
                return _gameDataProvider.SetPlayerTurn(gameId, game.Players[playerIndex]); 
            }
            return false;
        }

        public bool SaveGameStatus(string gameId, GameStatus status)
        {
            var game = GetGame(gameId);
            if (game != null)
            {
                game.Status = status.ToString();
            }
            return _gameDataProvider.SaveGameStatus(gameId, status);
        }

        public bool SetGameWinner(string gameId, string playerId)
        {
            var game = GetGame(gameId);
            if (game != null)
            {
                game.Winner = playerId;
            }
            return _gameDataProvider.SetGameWinner(gameId, playerId);
        }

        public bool UpdatePlayerProfile(string playerId, int points, out int level, out int totalPoints)
        {
            return _gameDataProvider.UpdatePlayerProfile(playerId, points, out level, out totalPoints);
        }

        public bool SaveGamePlayerStatus(string gameId, Player player)
        {
            return _gameDataProvider.SaveGamePlayerStatus(gameId, player);
        }

        private Game GetBotGame(UserSession session)
        {
            //List<string> myTiles = _movesDataProvider.GetRandomTiles(6, new List<string>());
            //Game game = _gameDataProvider.CreateGame(session.Account.Id, GameStatus.Started, true, myTiles);
            //game.Players.Add(new Player(session.Account)
            //{
            //    Points = 0,
            //    IsHost = true,
            //    IsActive = true,
            //    TilesRemaining = 50,
            //});
            //List<string> botTiles = _movesDataProvider.GetRandomTiles(6, new List<string>());
            //_gameDataProvider.StartGame(game.Id, AppConfig.BotUserId, false, botTiles);
            //game.Players.Add(new Player()
            //{
            //    Id = AppConfig.BotUserId,
            //    UserName = "bot",
            //    FirstName = "Mahesh",
            //    LastName = "Tokle",
            //    Points = 0,
            //    IsHost = true,
            //    IsActive = false,
            //    TilesRemaining = 50,
            //    Level = 1
            //});
            //return game;
            return null;
        }

        private Game GetRandomGame(UserSession session, ref List<string> gameTiles)
        {
            List<string> tiles = _movesDataProvider.GetRandomTiles(6,ref gameTiles); 
            Game game = null;
            bool isGameFound = false;
            List<string> waitingList = _gameDataProvider.GetWaitingGamePlayers(session.ActiveGames);
            if (waitingList.Count > 0)
            {
                game = GetGame(waitingList[0]);
                lock (_lockObj)
                {
                    var exGameTiles = GetGameTiles(game.Id);
                    tiles = _movesDataProvider.GetRandomTiles(6, ref exGameTiles);
                    isGameFound = _gameDataProvider.StartGame(game.Id, session.Account.Id, GameStatus.Started, true, tiles, exGameTiles);
                    if (!isGameFound)
                    {
                        waitingList = _gameDataProvider.GetWaitingGamePlayers(session.ActiveGames);
                        if (waitingList.Count > 0)
                        {
                            game = GetGame(waitingList[0]);
                            exGameTiles = GetGameTiles(game.Id);
                            tiles = _movesDataProvider.GetRandomTiles(6, ref exGameTiles);
                            isGameFound = _gameDataProvider.StartGame(game.Id, session.Account.Id, GameStatus.Started, true, tiles, exGameTiles);
                        }
                    }
                    if (isGameFound)
                    {
                        gameTiles = exGameTiles;
                        game.Players.Add(new Player(session.Account)
                        {
                            IsActive = true,
                            IsHost = false,
                            Tiles = tiles,
                            TilesRemaining = AppConfig.MaxTilesPerPlayer
                        });
                        game.Status = GameStatus.Started.ToString();
                    }
                }
            }
            if (!isGameFound)
            {
                game = _gameDataProvider.CreateGame(session.Account.Id, GameStatus.Waiting, false, tiles, gameTiles); // host the game
            }
            return game;
        }

        private Game PlayWith(UserSession session, string playWith, string gameId, ref List<string> gameTiles)
        {
            Game game = null;
            if (string.IsNullOrEmpty(gameId))
            {
                List<string> myTiles = _movesDataProvider.GetRandomTiles(6, ref gameTiles);
                game = _gameDataProvider.CreateGame(session.Account.Id, GameStatus.Invited, false, myTiles,gameTiles);
                
                myTiles = _movesDataProvider.GetRandomTiles(6, ref gameTiles);
                _gameDataProvider.StartGame(game.Id, playWith, GameStatus.Invited, true, myTiles, gameTiles);
                
                SetInvite(playWith, game.Id); // set the invite for the user in the cache.
            }
            else
            {
                var invatedGame = GetGame(gameId);
                _gameDataProvider.SaveGameStatus(gameId, GameStatus.Started);
                invatedGame.Status = GameStatus.Started.ToString();
                game = invatedGame;
            }
            return game;
        }

        public List<string> GetGameTiles(string gameId)
        {
            var tiles = _cacheProvider.GetValue("GameTiles" + gameId) as List<string>;
            if (tiles == null)
            {
                Dictionary<string, List<string>> gameTiles = _gameDataProvider.GetGameTiles(new List<string>(){gameId});
                _cacheProvider.AddValue("GameTiles" + gameId, gameTiles[gameId]);
                return gameTiles[gameId];
            }
            return tiles;
        }

        public bool SetGameTiles(string gameId, List<string> gameTiles, bool save = true)
        {
            //Set game tiles in the cache
            _cacheProvider.AddValue("GameTiles" + gameId, gameTiles);
            //Save game tiles in the appacitive
            if(save)
                _gameDataProvider.SaveGameTiles(gameId, gameTiles);
            return true;
        }

        public Game GetGame(string gameId)
        {
            var game = _cacheProvider.GetValue(gameId) as Game;
            if (game == null)
            {
                game = _gameDataProvider.GetGameInfo(new List<string>() {gameId}).FirstOrDefault();
                _cacheProvider.AddValue(gameId, game);
            }
            return game;
        }

        public List<Move> GetGameMoves(string gameid)
        {
            var moves = _cacheProvider.GetValue("GameMoves" + gameid) as List<Move>;
            if (moves == null)
            {
                moves = _movesDataProvider.GetNextMoves(gameid, new List<string>());
                _cacheProvider.AddValue("GameMoves" + gameid, moves);
            }
            return moves;
        }

        public List<Game> LoadGamesFromAppacitive(List<string> gameIds)
        {
            List<Game> myGames = new List<Game>();
            List<string> gamesToLoad = new List<string>();
            Game cachedGame = null;
            foreach (string gameId in gameIds)
            {
                cachedGame = _cacheProvider.GetValue(gameId) as Game;
                if(cachedGame == null)
                    gamesToLoad.Add(gameId);
                else
                    myGames.Add(cachedGame);
            }
            var games = _gameDataProvider.GetGameInfo(gamesToLoad);
            foreach (Game game in games)
            {
                _cacheProvider.AddValue(game.Id, game);
                myGames.Add(game);
            }
            return myGames;
        }

        public List<string> GetInvites(string playerId)
        {
            var invites = _cacheProvider.GetValue("Invites") as Dictionary<string, List<string>>;
            if (invites == null)
            {
                lock (_lockInvites)
                {
                    invites = _cacheProvider.GetValue("Invites") as Dictionary<string, List<string>>;
                    if (invites == null)
                    {
                        invites = new Dictionary<string, List<string>>();
                        var gameInvites = _gameDataProvider.GetUserInvites();
                        foreach (var game in gameInvites)
                        {
                            var invitedTo = game.Players.Find(p => p.IsActive == true);
                            if (invitedTo != null)
                            {
                                if (!invites.ContainsKey(invitedTo.Id))
                                {
                                    invites[invitedTo.Id] = new List<string>();
                                }
                                invites[invitedTo.Id].Add(game.Id);
                            }
                        }
                        _cacheProvider.AddValue("Invites", invites);
                    }
                }
            }
            if (invites.ContainsKey(playerId))
            {
                return invites[playerId];
            }
            else
            {
                return new List<string>();
            }
        }

        public void SetInvite(string playerId, string gameId)
        {
            var invites = _cacheProvider.GetValue("Invites") as Dictionary<string, List<string>>;
            if (invites == null)
            {
                GetInvites(playerId);
                invites = _cacheProvider.GetValue("Invites") as Dictionary<string, List<string>>;
            }
            if (invites != null)
            {
                lock (_lockInvites)
                {
                    if (!invites.ContainsKey(playerId))
                    {
                        invites[playerId] = new List<string>();
                    }
                    invites[playerId].Add(gameId);
                }
            }
        }
    }
}
