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
    public class MovesService : IMovesService
    {
        private const string Source = "MovesService";
        public GetGameMovesRs GetGameMoves(GetGameMovesRq request)
        {
            if (request == null || string.IsNullOrEmpty(request.SessionId) || string.IsNullOrEmpty(request.GameId))
                return new GetGameMovesRs() { IsSucess = false, ErrorMessage = "Invalid GetGameMovesRq request!" };
            var response = new GetGameMovesRs() { IsSucess = true, GameId = request.GameId, Moves = new List<Move>()};
            try
            {
                IGameProvider gameProvider = GameProviderFactory.GetGameProvider();
                var session = GetSession(request.SessionId, response);
                if (session == null)
                    return response;
                var gameIndex = session.ActiveGames.FindIndex(g => string.Equals(g, request.GameId));
                var game = gameProvider.GetGame(request.GameId);
                if (gameIndex != -1 && game!=null)
                {
                    List<Model.Move> moves = new List<Model.Move>();
                    
                    //Check if player got the turn
                    var playerIndex = game.Players.FindIndex(p => string.Equals(p.Id, session.Account.Id));
                    var me = game.Players[playerIndex];
                    if (request.SendAll || me == null || me.IsActive)
                    {
                        //Get the ex moves
                        session.GameMoves = session.GameMoves ?? new Dictionary<string, List<string>>();
                        session.GameMoves[request.GameId] = session.GameMoves.ContainsKey(request.GameId) ? session.GameMoves[request.GameId] : new List<string>();
                        
                        //Get the new moves from cache
                        var gameMoves = gameProvider.GetGameMoves(game.Id);
                        moves = gameMoves.FindAll(m => !session.GameMoves[game.Id].Contains(m.Id));
                        moves = moves.OrderBy(m => m.TimeStamp).ToList();
                        session.GameMoves[request.GameId].AddRange(moves.Select(m => m.Id));
                        gameMoves = gameMoves.OrderBy(m => m.TimeStamp).ToList();
                        response.Moves = request.SendAll ? gameMoves.ToDataContract() : moves.ToDataContract();
                    }
                    response.IsMyTurn = response.Moves.Count > 0 && !string.Equals(response.Moves.Last().Player, session.Account.Id);
                    if (response.Moves.Count > 0)
                    {
                        response.Game = game.ToDataContract(session.Account.Id);
                    }
                }
                else
                {
                    response.IsSucess = false;
                    response.ErrorMessage = "Invalid game! Please refresh.";
                }
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Failed to get game moves!";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "GetGameMoves", Model.Severity.Critical);
            }
            return response;
        }

        public PlayMoveRs PlayMove(PlayMoveRq request)
        {
            if (request == null || string.IsNullOrEmpty(request.SessionId) || string.IsNullOrEmpty(request.GameId))
                return new PlayMoveRs() { IsSucess = false, ErrorMessage = "Invalid GetGameMovesRq request!" };
            var response = new PlayMoveRs() { IsSucess = true };
            try
            {
                IMovesProvider movesProvider = MovesProviderFactory.GetGameProvider();
                IGameProvider gameProvider = GameProviderFactory.GetGameProvider();
                var session = GetSession(request.SessionId, response);
                string errorMessage = string.Empty;
                var userMove = movesProvider.PlayUserMove(request.SessionId
                                                        , request.GameId
                                                        , new Model.Move() { MoveCode = request.MoveCode, Points = request.Points, Player = session.Account.Id}
                                                        , out errorMessage);
                if (userMove == null || !string.IsNullOrEmpty(errorMessage))
                {
                    response.IsSucess = false;
                    response.ErrorMessage = errorMessage;
                }
                else
                {
                    response.Game = gameProvider.GetGame(request.GameId).ToDataContract(session.Account.Id);
                    response.Move = userMove.ToDataContract();
                    response.TotalPoints = session.Account.Points;
                    response.Level = session.Account.Level;
                }
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Failed to save your move!";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "PlayMove", Model.Severity.Critical);
            }
            return response;
        }

        public PassChanceRs PassChance(PassChanceRq request)
        {
            if (request == null || string.IsNullOrEmpty(request.SessionId) || string.IsNullOrEmpty(request.GameId))
                return new PassChanceRs() { IsSucess = false, ErrorMessage = "Invalid PassChanceRq request!" };
            var response = new PassChanceRs() { IsSucess = true, GameId = request.GameId };
            try
            {
                IGameProvider gameProvider = GameProviderFactory.GetGameProvider();
                var session = GetSession(request.SessionId, response);
                if (session == null)
                    return response;
                var gameIndex = session.ActiveGames.FindIndex(g => string.Equals(g, request.GameId));
                var game = gameProvider.GetGame(request.GameId);
                if (gameIndex != -1 && game!=null)
                {
                    IMovesDataProvider movesDataProvider = MovesDataProviderFactory.GetMovesDataProvider();
                    Model.Move move = new Model.Move() { Points = 0, MoveCode = "", Player = session.Account.Id };
                    move = movesDataProvider.SaveMove(request.GameId, move);
                    var gameMoves = gameProvider.GetGameMoves(request.GameId);
                    gameMoves.Add(move);
                    session.GameMoves[request.GameId].Add(move.Id);

                    var pIndex = game.Players.FindIndex(p => string.Equals(p.Id, session.Account.Id));
                    var player = game.Players[pIndex];
                    player.IsActive = false;
                    gameProvider.SetGamePlayerTurn(request.GameId, player.Id, false);

                    pIndex = game.Players.FindIndex(p => !string.Equals(p.Id, session.Account.Id));
                    var opponant = game.Players[pIndex];
                    opponant.IsActive = true;
                    gameProvider.SetGamePlayerTurn(request.GameId, opponant.Id, true);
                }
                else
                {
                    response.IsSucess = false;
                    response.ErrorMessage = "Invalid game! Please refresh.";
                }
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Failed to pass your chance!";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "PassChance", Model.Severity.Critical);
            }
            return response;
        }

        private static Model.UserSession GetSession(string sessionId, Response response)
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

        public GetMyTilesRs GetMyTiles(GetMyTilesRq request)
        {
            //if (request == null || string.IsNullOrEmpty(request.SessionId) || string.IsNullOrEmpty(request.GameId))
            //    return new GetMyTilesRs() { IsSucess = false, ErrorMessage = "Invalid GetMyTilesRq request!" };
            //var response = new GetMyTilesRs() { IsSucess = true, GameId = request.GameId };
            //try
            //{
            //    var session = GetSession(request.SessionId, response);
            //    if (session == null)
            //        return response;
            //    var gameIndex = session.ActiveGames.FindIndex(g => string.Equals(g, request.GameId));
            //    var game = 
            //    if (gameIndex != -1)
            //    {
            //        var player = session.ActiveGames[gameIndex].Players.Find(p => string.Equals(p.Id, session.Account.Id));
            //        player.Tiles = player.Tiles ?? new List<string>();
            //        response.Tiles = player.Tiles;
            //        response.TilesRemaining = player.TilesRemaining;
            //    }
            //    else
            //    {
            //        response.IsSucess = false;
            //        response.ErrorMessage = "Invalid game! Please refresh.";
            //    }
            //}
            //catch (Exception ex)
            //{
            //    response.IsSucess = false;
            //    response.ErrorMessage = ex.Message;
            //}
            return null;
        }
    }
}
