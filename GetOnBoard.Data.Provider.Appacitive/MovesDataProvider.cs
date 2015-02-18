using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Appacitive.Sdk;
using GetOnBoard.Core.Model;
using GetOnBoard.Data.Interfaces;
using GetOnBoard.Data.Provider.Appacitive.AppHelper;
using GetOnBoard.Data.Provider.Appacitive.Constants;
using GetOnBoard.Data.Provider.Appacitive.Extensions;

namespace GetOnBoard.Data.Provider.Appacitive
{
    public class MovesDataProvider : IMovesDataProvider
    {
        private Random _random = null;
        readonly List<string> _colors = new List<string>() { "g", "b", "p", "gr", "br", "v" };
        readonly List<string> _symbols = new List<string>() { "1", "2", "3", "4", "5", "6" };

        public MovesDataProvider()
        {
            AppInitializer.Initialize();
            _random = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber));
        }

        public List<string> GetRandomTiles(int cnt, ref List<string> gameTiles)
        {
            List<string> myTiles = new List<string>();
            for (int i = 0; i < cnt; i++)
            {
                if(gameTiles.Count == 0)
                    break;
                int index = _random.Next(gameTiles.Count);
                myTiles.Add(gameTiles[index]);
                gameTiles.RemoveAt(index);
            }
            return myTiles;
        }

        public List<Move> GetNextMoves(string gameId, List<string> existingIds)
        {
            Article game = new Article(Schemas.Game, gameId);
            var moveArticleIds = game.GetAllConnectedArticles(Relations.GameMove, null, null,new[] {"__id"});
            var moveIds = moveArticleIds.Select(m => m.Id).ToList();
            moveIds = moveIds.Except(existingIds).ToList();
            List<Move> moves = new List<Move>();
            if (moveIds.Count > 0)
            {
                var graphMoves = Graph.Project("get_next_moves", moveIds, null).Result;
                moves = graphMoves.Select(node => node.Article).ToList().ToModelMoves();
            }
            return moves;
        }


        public Move SaveMove(string gameId, Move move)
        {
            Article moveArticle = new Article(Schemas.Moves);
            moveArticle.Set("move_code", move.MoveCode);
            moveArticle.Set("points", move.Points);
            moveArticle.Set("player", move.Player);
            var con = Connection.New(Relations.GameMove)
                .FromExistingArticle(Schemas.Game, gameId)
                .ToNewArticle(Schemas.Moves, moveArticle);
            con.SaveAsync().Wait();
            move = moveArticle.ToModelMove();
            move.TimeStamp = DateTime.UtcNow;
            return move;
        }

        public List<string> GenerateGameTiles(int totalSets)
        {
            var counter = 0;
            List<string> gameTiles = new List<string>();
            do
            {
                foreach (var color in _colors)
                {
                    foreach (var symbol in _symbols)
                    {
                        gameTiles.Add(string.Format("{0}_{1}",color,symbol));
                    }
                    counter++;
                    if(counter >= AppConfigurations.TileSetCountPerGame)
                        break;
                }
            } while (counter < totalSets);
            List<string> random1 = new List<string>();
            List<string> random2 = new List<string>();
            bool isRand1 = true;
            for (int i = 0; i < gameTiles.Count; i++)
            {
                if (isRand1)
                {
                    random1.Add(gameTiles[i]);
                    isRand1 = false;
                }
                else
                {
                    random2.Add(gameTiles[i]);
                    isRand1 = true;
                }
            }
            random1.AddRange(random2);
            gameTiles = random1;
            return gameTiles;
        }
    }
}
