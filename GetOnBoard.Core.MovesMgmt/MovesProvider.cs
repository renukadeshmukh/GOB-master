using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetOnBoard.Core.Interfaces;
using GetOnBoard.Core.Model;
using GetOnBoard.Data.Interfaces;

namespace GetOnBoard.Core.MovesMgmt
{
    public class MovesProvider : IMovesProvider
    {
        private readonly ICacheProvider _cacheProvider = null;
        private readonly IGameProvider _gameProvider = null;
        private readonly IMovesDataProvider _movesDataProvider = null;
        public MovesProvider(ICacheProvider cacheProvider , IGameProvider gameProvider , IMovesDataProvider movesDataProvider )
        {
            _cacheProvider = cacheProvider;
            _gameProvider = gameProvider;
            _movesDataProvider = movesDataProvider;
        }

        public Move PlayUserMove(string sessionId, string gameId, Move userMove, out string errorMessage)
        {
            errorMessage = string.Empty;
            //Get the requested game from the session
            var session = _cacheProvider.GetValue(sessionId) as UserSession;
            if (session == null)
            {
                errorMessage = "Session expired! Please login again.";
                return null;
            }
            var game = _gameProvider.GetGame(gameId);
            if (game != null)
            {
                game.LastActivityTime = DateTime.UtcNow;
                session.GameMoves = session.GameMoves ?? new Dictionary<string, List<string>>();
                session.GameMoves[gameId] = session.GameMoves.ContainsKey(gameId) ? session.GameMoves[gameId] : new List<string>();
                var moves = _gameProvider.GetGameMoves(gameId);
                
                //Validate the user move
                if (!ValidMove(userMove, moves, out errorMessage) || !string.IsNullOrEmpty(errorMessage))
                {
                    return null;
                }

                //Save user move in database and session moves
                userMove = _movesDataProvider.SaveMove(gameId, userMove);
                moves.Add(userMove);
                session.GameMoves[gameId].Add(userMove.Id);
                
                //Get the player next tiles and add them to the player tiles
                var pIndex = game.Players.FindIndex(p => string.Equals(p.Id, userMove.Player));
                var player = game.Players[pIndex];
                pIndex = game.Players.FindIndex(p => !string.Equals(p.Id, userMove.Player));
                var opponant = game.Players[pIndex];
                GiveNextTilesToPlayer(game, ref player, userMove);
                
                //Save player tiles and points
                player.Points = player.Points + userMove.Points;
                player.IsActive = false;
                _gameProvider.SaveGamePlayerStatus(gameId, player);

                //If game is finished then save game status and game winner.
                if (player.TilesRemaining <= 0)
                {
                    SaveGameStatus(gameId, game, opponant, player, session);
                }
                else
                {
                    //set the opponant status to active.
                    opponant.IsActive = true;
                    _gameProvider.SetGamePlayerTurn(gameId, opponant.Id, true);
                }
            }
            return userMove;
        }

        public Move PlayBot(UserSession session, int gameIndex)
        {
            var game = _gameProvider.GetGame(session.ActiveGames[gameIndex]);
            var bot = game.Players.Find(p => string.Equals(p.Id, AppConfig.BotUserId));
            Move botMove = PlayBotMove(session, bot, gameIndex);
            return botMove;
        }

        #region private functions
        private void SaveGameStatus(string gameId, Game game, Player opponant, Player player, UserSession session)
        {
            game.Status = GameStatus.Finished.ToString();
            _gameProvider.SaveGameStatus(gameId, GameStatus.Finished);
            var tilesDiff = opponant.Tiles.Count;
            player.Points = player.Points + tilesDiff;
            opponant.Points = opponant.Points - tilesDiff;

            var winner = player.Points >= opponant.Points ? player : opponant;
            _gameProvider.SetGameWinner(gameId, winner.Id);
            
            int totalPoints = 0, level = 0;
            _gameProvider.UpdatePlayerProfile(winner.Id, winner.Points, out level, out totalPoints);
            if (string.Equals(session.Account.Id, winner.Id))
            {
                session.Account.Points = totalPoints;
                session.Account.Level = level;
            }
        }

        private void GiveNextTilesToPlayer(Game game, ref Player player, Move userMove)
        {
            List<string> gameTiles = _gameProvider.GetGameTiles(game.Id);
            var parts = userMove.MoveCode.Split('|');
            foreach (var part in parts)
            {
                var mvs = part.Split(',');
                player.Tiles.Remove(mvs[2]);
            }
            player.TilesRemaining = player.TilesRemaining - parts.Length;
            if (player.TilesRemaining > 6)
                player.Tiles.AddRange(_movesDataProvider.GetRandomTiles(parts.Length, ref gameTiles));
            else
                player.Tiles.AddRange(_movesDataProvider.GetRandomTiles(player.TilesRemaining - player.Tiles.Count, ref gameTiles));
            _gameProvider.SetGameTiles(game.Id, gameTiles);
        }

        private bool ValidMove(Move userMove, List<Move> gameMoves, out string errorMessage)
        {
            errorMessage = string.Empty;
            var currentTiles = ToGameTiles(new List<Move>() { userMove });
            var gameTiles = ToGameTiles(gameMoves);
            var movedTiles = new List<GameTile>();
            foreach (var currentTile in currentTiles)
            {
                if (!IsValidMove(currentTile, gameTiles, movedTiles, out errorMessage) ||
                    !string.IsNullOrEmpty(errorMessage))
                {
                    return false;
                }
                gameTiles.Add(currentTile);
                movedTiles.Add(currentTile);
            }
            var lastMove = movedTiles[movedTiles.Count - 1];
            movedTiles.RemoveAt(movedTiles.Count - 1);
            int calculatedPoints = CalculatePoints(gameTiles, lastMove, movedTiles);
            if (calculatedPoints != userMove.Points)
            {
                errorMessage = "Points calculation doesnt match! Please refresh the page.";
                return false;
            }
            return true;
        }

        private Move PlayBotMove(UserSession session, Player bot, int gameIndex)
        {
            var game = _gameProvider.GetGame(session.ActiveGames[gameIndex]);
            var moves = _gameProvider.GetGameMoves(game.Id);
            var gameTiles = ToGameTiles(moves);
            List<string> tilesToBePlayed = GetMaxTilesCanBePlayed(bot.Tiles);
            int maxPoints = 0;
            List<GameTile> nextMoves = new List<GameTile>();
            foreach (var tile in tilesToBePlayed)
            {
                
                //Get all tile moves which match the color of the tile
                List<GameTile> matchingGameTiles = GetMatchingTileMoves(tile, gameTiles);
                //for each tile move get all sorrounding empty tile positions.
                foreach (var matchingGameTile in matchingGameTiles)
                {
                    //Get all empty position for the matching tile.
                    List<GameTile> emptyPositions = GetEmptyTilePositions(gameTiles, matchingGameTile, tile);
                    //for each empty position place all the tiles to be played in all direction and get the maximum points.
                    var myTiles = new List<string>();
                    myTiles.AddRange(tilesToBePlayed);
                    var tileIndex = myTiles.FindIndex(t => t == tile);
                    myTiles.RemoveAt(tileIndex);
                    foreach (var emptyPosition in emptyPositions)
                    {
                        GetMaxPoints("-x", gameTiles, tile, myTiles, emptyPosition, matchingGameTile, ref maxPoints, ref nextMoves);
                        GetMaxPoints("+x", gameTiles, tile, myTiles, emptyPosition, matchingGameTile, ref maxPoints, ref nextMoves);
                        GetMaxPoints("-y", gameTiles, tile, myTiles, emptyPosition, matchingGameTile, ref maxPoints, ref nextMoves);
                        GetMaxPoints("+y", gameTiles, tile, myTiles, emptyPosition, matchingGameTile, ref maxPoints, ref nextMoves);
                    }
                }
            }
            //parse nextmoves into bot move and return
            Move move = new Move() {Player = bot.Id, Points = maxPoints};
            List<string> tileMoves = new List<string>();
            foreach (var nextMove in nextMoves)
            {
                tileMoves.Add(string.Format("{0},{1},{2}", nextMove.Row, nextMove.Col, nextMove.Tile));
            }
            move.MoveCode = string.Join("|", tileMoves);
            return move;
        }

        private List<GameTile> ToGameTiles(List<Move> moves)
        {
            List<GameTile> gameTiles = new List<GameTile>();
            foreach (var move in moves)
            {
                if(string.IsNullOrWhiteSpace(move.MoveCode))
                    continue;
                string[] tiles = move.MoveCode.Split('|');
                foreach (var tile in tiles)
                {
                    string[] parts = tile.Split(',');
                    gameTiles.Add(new GameTile()
                    {
                        Row = int.Parse(parts[0]),
                        Col = int.Parse(parts[1]),
                        Tile = parts[2]
                    });
                }
            }
            return gameTiles;
        }

        private void GetMaxPoints(string direction, List<GameTile> gameTiles, string tile, List<string> myTiles, GameTile emptyPosition, GameTile selectedMove, ref int maxPoints, ref List<GameTile> nextMoves)
        {
            int points = 0;
            var currentMoves = new List<GameTile>();
            List<List<string>> tileCombinations = GetMytileCombinations(tile,myTiles);
            
            foreach (var tileCombination in tileCombinations)
            {
                points = 0;
                for (int i = 0; i < tileCombination.Count; i++)
                {
                    var nextMove = new GameTile() { Row = emptyPosition.Row, Col = emptyPosition.Col, Tile = tileCombination[i]};
                    if (direction == "-x")
                        nextMove.Col = nextMove.Col - i;
                    if (direction == "+x")
                        nextMove.Col = nextMove.Col + i;
                    if (direction == "-y")
                        nextMove.Row = nextMove.Row - i;
                    if (direction == "+y")
                        nextMove.Row = nextMove.Row - i;
                    string errorMessage = string.Empty;
                    if (nextMove.Row <= 16 && nextMove.Row >-1 && nextMove.Col <=16 && nextMove.Col >-1 && IsValidMove(nextMove, gameTiles, currentMoves, out errorMessage))
                    {
                        points = points + CalculatePoints(gameTiles, nextMove, currentMoves);
                    }
                    else
                    {
                        break;
                    }
                }
                if (points > maxPoints)
                {
                    maxPoints = points;
                    nextMoves = currentMoves;
                }
            }
        }

        private int CalculatePoints(List<GameTile> gameTiles, GameTile nextMove, List<GameTile> currentMoves)
        {
            currentMoves.Add(nextMove);
            if (nextMove.Row == 8 && nextMove.Col == 8)
                return 1;
            int points = 0;
            var isHor = InSameLine("hor", nextMove.Row, currentMoves);
            var tiles = GetRowTiles(isHor, nextMove.Row, nextMove.Col, gameTiles);
            points = points + (tiles.Count == 0 ? 0 : tiles.Count + 1);
            var bonus = points == 6 ? 6 : 0;
            foreach (var currentMove in currentMoves)
            {
                tiles = GetRowTiles(!isHor, currentMove.Row, currentMove.Col, gameTiles);
                points = points + (tiles.Count == 0 ? 0 : tiles.Count + 1);
                if ((tiles.Count + 1)==6) 
                {
                    bonus = bonus + 6;
                }
            }
            points = points + bonus;
            return points;
        }

        private List<List<string>> GetMytileCombinations(string tile,List<string> myTiles)
        {
            List<List<string>> tileCombinations = new List<List<string>>();
            int startPosition = -1;
            foreach (var myTile in myTiles)
            {
                startPosition++;
                List<string> tileCombination = new List<string>();
                for (int i = 0; i < myTiles.Count; i++)
                {
                    tileCombination.Add(myTiles[(startPosition+i)%myTiles.Count]);
                }
                tileCombinations.Add(tileCombination);
            }
            foreach (var tileCombination in tileCombinations)
            {
                tileCombination.Insert(0, tile);
            }
            return tileCombinations;
        }

        private List<GameTile> GetEmptyTilePositions(List<GameTile> gameTiles, GameTile matchingGameTile, string tile)
        {
            //Empty and valid move
            List<GameTile> emptyPositions = new List<GameTile>();
            GameTile nextMove = GetValidEmptyTile(gameTiles, matchingGameTile.Row + 1, matchingGameTile.Col, tile);
            if(nextMove != null)
                emptyPositions.Add(nextMove);
            nextMove = GetValidEmptyTile(gameTiles, matchingGameTile.Row - 1, matchingGameTile.Col, tile);
            if (nextMove != null)
                emptyPositions.Add(nextMove);
            nextMove = GetValidEmptyTile(gameTiles, matchingGameTile.Row, matchingGameTile.Col + 1, tile);
            if (nextMove != null)
                emptyPositions.Add(nextMove);
            nextMove = GetValidEmptyTile(gameTiles, matchingGameTile.Row, matchingGameTile.Col - 1, tile);
            if (nextMove != null)
                emptyPositions.Add(nextMove);
            return emptyPositions;
        }

        private GameTile GetValidEmptyTile(List<GameTile> gameTiles, int row, int col, string tile)
        {
            GameTile nextMove = null;
            if (row < 17 && row >=0 && col < 17 && col >=0)
            {
                nextMove = new GameTile() { Row = row, Col = col, Tile = tile };
                string errorMessage;
                if (IsValidMove(nextMove, gameTiles, new List<GameTile>(),  out errorMessage))
                {
                    return nextMove;
                }
            }
            return nextMove;
        }

        private bool IsValidMove(GameTile nextMove, List<GameTile> gameTiles, List<GameTile> currentTiles, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (gameTiles.Count == 0 && nextMove.Row == 8 && nextMove.Col == 8)
            {
                return true;
            }
            var foundTile = gameTiles.Find(gt => gt.Row == nextMove.Row && gt.Col == nextMove.Col);
            if (foundTile != null)
            {
                errorMessage = "Invalid move! Tile cant be placed on some other tile.";
            }
            var verTiles = GetRowTiles(true, nextMove.Row, nextMove.Col, gameTiles);
            var horTiles = GetRowTiles(false, nextMove.Row, nextMove.Col, gameTiles);
            if (horTiles.Count == 0 && verTiles.Count == 0)
            {
                errorMessage = "Invalid Move! Tile should be placed near some other tile.";
                return false;
            }
            if (horTiles.Exists( t => string.Equals(t.Tile, nextMove.Tile)))
            {
                errorMessage = "Invalid move! Same tile exist in the horizontal row.";
                return false;
            }
            if (verTiles.Exists(t => string.Equals(t.Tile, nextMove.Tile)))
            {
                errorMessage = "Invalid move! Same tile exist in the verticle row.";
                return false;
            }
            var isValidRow = IsValidTiles(horTiles, nextMove.Tile);
            var isValidCol = IsValidTiles(verTiles, nextMove.Tile);
            if (!isValidRow || !isValidCol)
            {
                errorMessage = "Invalid move! " + (isValidCol ? "Horizontal" : "Verticle") + " tiles doesnt match.";
                return false;
            }
            var inSameRow = InSameLine("hor", nextMove.Row, currentTiles);
            var inSameCol = InSameLine("ver", nextMove.Col, currentTiles);
            if (!inSameRow && !inSameCol)
            {
                errorMessage = "Invalid Move direction!";
                return false;
            }
            foreach (var currentTile in currentTiles)
            {
                if (horTiles.FindIndex(h =>
                            string.Equals(h.Tile, currentTile.Tile) && h.Row == currentTile.Row && h.Col == currentTile.Col) == -1 &&
                    verTiles.FindIndex(v =>
                            string.Equals(v.Tile, currentTile.Tile) && v.Row == currentTile.Row && v.Col == currentTile.Col) == -1)
                {
                    errorMessage = "Invalid Move! Tile should be placed near other tiles you placed in this move";
                    return false;
                }
            }
            return true;
        }

        private bool InSameLine(string direct, int rowCol, List<GameTile> currentTiles)
        {
            foreach (var currentTile in currentTiles)
            {
                if (direct == "hor" ? currentTile.Row != rowCol : currentTile.Col != rowCol)
                {
                    return false;
                }  
            }
            return true;
        }

        private bool IsValidTiles(List<GameTile> tiles, string tile)
        {
            var color = tile.Split('_')[0];
            var invalidColor = tiles.Find(t => !string.Equals(t.Tile.Split('_')[0], color));
            if (invalidColor != null)
            {
                var symbol = tile.Split('_')[1];
                var invalidSymbol = tiles.Find(t => !string.Equals(t.Tile.Split('_')[1], symbol));
                if (invalidSymbol != null)
                {
                    return false;
                }
            }
            return true;
        }

        private List<GameTile> GetRowTiles(bool isHor, int row, int col, List<GameTile> gameTiles)
        {
            List<GameTile> tiles = new List<GameTile>();
            int pos = isHor ? col : row;
            for (int i = 1; i < 17; i++)
            {
                if(pos+i>=17)
                    break;
                var tile = gameTiles.Find(t => isHor ? (t.Row == row && t.Col == (pos + i)) : (t.Row == (pos + i) && t.Col == col));
                if (tile != null)
                {
                    tiles.Add(tile);
                }else break;
            }
            for (int i = 1; i < 17; i++)
            {
                if (pos - i <=-1)
                    break;
                var tile = gameTiles.Find(t => isHor ? (t.Row == row && t.Col == (pos - i)) : (t.Row == (pos - i) && t.Col == col));
                if (tile != null)
                {
                    tiles.Add(tile);
                }else break;
            }
            return tiles;
        }

        private List<GameTile> GetMatchingTileMoves(string tile, List<GameTile> gameTiles)
        {
            List<GameTile> matchingTileMoves = new List<GameTile>();
            string tileColor = tile.Split('_')[0];
            string tileSymbol = tile.Split('_')[0];
            foreach (var gameTile in gameTiles)
            {
                string color = gameTile.Tile.Split('_')[0];
                string symbol = gameTile.Tile.Split('_')[1];
                if (string.Equals(tileColor, color) || string.Equals(tileSymbol, symbol))
                {
                    matchingTileMoves.Add(gameTile);
                }
            }
            return matchingTileMoves;
        }
        
        private List<string> GetMaxTilesCanBePlayed(List<string> tiles)
        {
            string selectedColor = string.Empty;
            int maxColorCount = FindMaxTilePattern(tiles, 0, out selectedColor);
            string selectedSymbol = string.Empty;
            int maxSymbolCount = FindMaxTilePattern(tiles, 1, out selectedSymbol);
            List<string> tilesToBePlayed = new List<string>();
            if (maxColorCount >= maxSymbolCount)
            {
                tilesToBePlayed = tiles.Where(t => t.Split('_')[0] == selectedColor).ToList();
            }
            else
            {
                tilesToBePlayed = tiles.Where(t => t.Split('_')[1] == selectedSymbol).ToList();
            }
            return tilesToBePlayed;
        }

        private static int FindMaxTilePattern(List<string> tiles, int patternIndex, out string selectedPattern)
        {
            selectedPattern = string.Empty;
            int maxPatternCnt = 0;
            List<string> patterns = tiles.Select(t => t.Split('_')[patternIndex]).ToList();
            foreach (var pattern in patterns)
            {
                if (pattern == selectedPattern)
                    continue;
                var foundPatterns = patterns.FindAll(c => c == pattern);
                if (maxPatternCnt < foundPatterns.Count)
                {
                    maxPatternCnt = foundPatterns.Count;
                    selectedPattern = foundPatterns[0];
                }
            }
            return maxPatternCnt;
        }
        #endregion
    }
}
