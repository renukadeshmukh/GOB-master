using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetOnBoard.Core.Factories;
using GetOnBoard.Core.Interfaces;
using GetOnBoard.Services.DataContracts;
using GetOnBoard.Services.ServiceImpl.Translators;
using Model = GetOnBoard.Core.Model;
using GetOnBoard.Data.Factories;
using GetOnBoard.Data.Interfaces;
using GetOnBoard.Services.DataContracts.Messages;
using GetOnBoard.Services.ServiceContracts;

namespace GetOnBoard.Services.ServiceImpl
{
    public class GameService : IGameService
    {
        private const string Source = "GameService";
        public CreateGameRs CreateGame(CreateGameRq request)
        {
            if (request == null || string.IsNullOrEmpty(request.Method) || string.IsNullOrEmpty(request.SessionId))
                return new CreateGameRs() {ErrorMessage = "Invalid create game request!!", IsSucess = false};
            CreateGameRs response = new CreateGameRs() { IsSucess = true };
            try
            {
                IGameProvider gameProvider = GameProviderFactory.GetGameProvider();
                var session = GetSession(request.SessionId, response);
                string errorMessage = string.Empty;
                Model.Game game = gameProvider.CreateGame(request.SessionId, request.Method, request.PlayWith, request.GameId, out errorMessage);
                if (game != null && string.IsNullOrEmpty(errorMessage))
                {
                    response.Game = game.ToDataContract(session.Account.Id);
                }
                else
                {
                    response.IsSucess = false;
                    response.ErrorMessage = errorMessage;
                }
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Failed to create game!";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "CreateGame", Model.Severity.Critical);
            }
            return response;
        }

        public UserGamesRs GetUserGames(UserGamesRq request)
        {
            if (request == null || string.IsNullOrEmpty(request.SessionId))
                return new UserGamesRs() { ErrorMessage = "Invalid GetUserGames request!!", IsSucess = false };
            UserGamesRs response = new UserGamesRs() { IsSucess = true };
            try
            {
                IGameProvider gameProvider = GameProviderFactory.GetGameProvider();
                var session = GetSession(request.SessionId, response);
                if (session == null)
                    return response;
                if (session.ActiveGames != null)
                {
                    response.UserGames = new List<Game>();
                    foreach (var gameId in session.ActiveGames)
                    {
                        response.UserGames.Add(gameProvider.GetGame(gameId).ToDataContract(session.Account.Id));
                    }
                }
                else
                {
                    IGameDataProvider gameDataProvider = GameDataProviderFactory.GetGameDataProvider();
                    var myGames = gameDataProvider.GetUserActiveGames(session.Account.Id);
                    response.UserGames = gameProvider.LoadGamesFromAppacitive(myGames).ToDataContract(session.Account.Id);
                    session.ActiveGames = myGames;
                    var gameTiles = gameDataProvider.GetGameTiles(session.ActiveGames);
                    foreach (var activeGameId in session.ActiveGames)
                    {
                        gameProvider.SetGameTiles(activeGameId, gameTiles[activeGameId], false);
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Failed to pull your games!";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "GetUserGames", Model.Severity.Critical);
            }
            return response;
        }

        public GameStatusRs GetGameStatus(GameStatusRq request)
        {
            if (request == null || request.SessionId == null || request.GameIds == null)
                return new GameStatusRs() {IsSucess = false, ErrorMessage = "Invalid GetGameStatus request!"};
            var response = new GameStatusRs() {IsSucess = true};
            try
            {
                IGameProvider gameProvider = GameProviderFactory.GetGameProvider();
                var session = GetSession(request.SessionId, response);
                if (session == null)
                    return response;
                
                List<Model.Game> games = new List<Model.Game>();
                //Check for any invites and add them in the requested ids.
                var gameInvites = gameProvider.GetInvites(session.Account.Id);
                if (gameInvites != null)
                {
                    request.GameIds.AddRange(gameInvites);    
                }

                //Get the games from appacitive whose status is changed for me.
                if (request.GameIds.Count > 0)
                {
                    games = gameProvider.LoadGamesFromAppacitive(request.GameIds);
                }
                
                response.Games = games.ToDataContract(session.Account.Id);
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Failed to refresh the game status!";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "GetGameStatus", Model.Severity.Critical);
            }
            return response;
        }

        public GetTopPlayersRs GetTopPlayers(GetTopPlayersRq request)
        {
            if (request == null || request.SessionId == null)
                return new GetTopPlayersRs() { IsSucess = false, ErrorMessage = "Invalid GetTopPlayers request!" };
            var response = new GetTopPlayersRs() { IsSucess = true };
            try
            {
                var session = GetSession(request.SessionId, response);
                if (session == null)
                    return response;
                IGameDataProvider gameDataProvider = GameDataProviderFactory.GetGameDataProvider();
                response.TopPlayers = gameDataProvider.GetTopPLayers(request.Count).ToDataContract();
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Failed to pull top player list.";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "GetTopPlayers", Model.Severity.Normal);
            }
            return response;
        }

        public DeleteGameRs DeleteGame(DeleteGameRq request)
        {
            if (request == null || request.SessionId == null || string.IsNullOrEmpty(request.GameId))
                return new DeleteGameRs() { IsSucess = false, ErrorMessage = "Invalid DeleteGame request!" };
            var response = new DeleteGameRs() { IsSucess = true, GameId = request.GameId};
            try
            {
                IGameProvider gameProvider = GameProviderFactory.GetGameProvider();
                var session = GetSession(request.SessionId, response);
                if (session == null)
                    return response;
                var game = gameProvider.GetGame(request.GameId);
                if (game!=null)
                {
                    var isValidGamePlayer = game.Players.Exists(p => string.Equals(p.Id, session.Account.Id));
                    if (isValidGamePlayer && IsDeleteGameAllowed(game))
                    {
                        gameProvider.SaveGameStatus(request.GameId, Model.GameStatus.Cancelled);

                        Model.Player me = null;
                        Model.Player opponent = null;
                        foreach (var player in game.Players)
                        {
                            if (string.Equals(player.Id, session.Account.Id))
                            {
                                me = player;
                            }
                            else
                            {
                                opponent = player;
                            }
                        }
                        me.IsActive = false;
                        if (opponent != null)
                        {
                            opponent.IsActive = false;
                            gameProvider.SetGamePlayerTurn(request.GameId, opponent.Id, false);
                        }
                        gameProvider.SetGamePlayerTurn(request.GameId, me.Id, false);
                    }
                    else
                    {
                        response.IsSucess = false;
                        response.ErrorMessage = "Game can not be deleted! You should wait for other player to play at least one day.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Failed to delete game!";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "DeleteGame", Model.Severity.Normal);
            }
            return response;
        }

        public ResignGameRs ResignGame(ResignGameRq request)
        {
            if (request == null || request.SessionId == null || string.IsNullOrEmpty(request.GameId))
                return new ResignGameRs() { IsSucess = false, ErrorMessage = "Invalid ResignGame request!" };
            var response = new ResignGameRs() { IsSucess = true, GameId = request.GameId };
            try
            {
                IGameProvider gameProvider = GameProviderFactory.GetGameProvider();
                var session = GetSession(request.SessionId, response);
                if (session == null)
                    return response;
                var gameIndex = session.ActiveGames.FindIndex(g => string.Equals(g, request.GameId));
                var game = gameProvider.GetGame(request.GameId);
                if (gameIndex != -1)
                {
                    gameProvider.SaveGameStatus(request.GameId, Model.GameStatus.Resigned);
                    
                    var pIndex = game.Players.FindIndex(p => string.Equals(p.Id, session.Account.Id));
                    var player = game.Players[pIndex];
                    pIndex = game.Players.FindIndex(p => !string.Equals(p.Id, session.Account.Id));
                    var opponant = game.Players[pIndex];
                    int level, totalPoints;
                    gameProvider.UpdatePlayerProfile(opponant.Id
                                                        , opponant.Points
                                                        , out level
                                                        , out totalPoints);
                    player.IsActive = opponant.IsActive = false;
                    gameProvider.SetGamePlayerTurn(request.GameId, opponant.Id, false);
                    gameProvider.SetGamePlayerTurn(request.GameId, player.Id, false);
                    gameProvider.SetGameWinner(request.GameId, opponant.Id);
                }
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Failed to resign game!";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "ResignGame", Model.Severity.Normal);
            }
            return response;
        }

        public UserGamesRs GetPreviousGames(UserGamesRq request)
        {
            if (request == null || request.SessionId == null)
                return new UserGamesRs() { IsSucess = false, ErrorMessage = "Invalid GetPreviousGames request!" };
            var response = new UserGamesRs() { IsSucess = true };
            try
            {
                var session = GetSession(request.SessionId, response);
                if (session == null)
                    return response;
                IGameDataProvider gameDataProvider = GameDataProviderFactory.GetGameDataProvider();
                response.UserGames = gameDataProvider.GetPreviousGames(session.Account.Id).ToDataContract(session.Account.Id);
                if (response.UserGames != null)
                {
                    List<string> notStartedGames = new List<string>();
                    foreach (var userGame in response.UserGames)
                    {
                        if (userGame.Players.Count == 1)
                        {
                            notStartedGames.Add(userGame.Id);
                        }
                    }
                    response.UserGames.RemoveAll(g => notStartedGames.Contains(g.Id));
                }
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Failed to get your previous game history!";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "GetPreviousGames", Model.Severity.Normal);
            }
            return response;
        }

        #region private methods
        private Model.UserSession GetSession(string sessionId, Response response)
        {
            ICacheProvider cacheProvider = CacheProviderFactory.GetCacheProvider();
            var session = cacheProvider.GetValue(sessionId) as Model.UserSession;
            if (session == null)
            {
                response.IsSucess = false;
                response.Code = 4004;
                response.ErrorMessage = "Session Expired! Please login again.";
            }
            return session;
        }

        private bool IsDeleteGameAllowed(Model.Game game)
        {
            //return true;
            if(!string.Equals(game.Status, Model.GameStatus.Started.ToString(), StringComparison.OrdinalIgnoreCase))
                return true;
            var timeDiff = DateTime.Now.ToUniversalTime() - game.LastActivityTime;
            if (timeDiff.TotalDays >= 1)
            {
                return true;
            }
            return false;
        }

        
        #endregion
    }
}
