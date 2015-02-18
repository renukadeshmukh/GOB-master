using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Appacitive.Sdk;
using GetOnBoard.Core.Model;
using GetOnBoard.Data.Factories;
using GetOnBoard.Data.Interfaces;
using GetOnBoard.Data.Provider.Appacitive;
using GetOnBoard.Data.Provider.Appacitive.AppHelper;
using GetOnBoard.Data.Provider.Appacitive.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GetOnBoard.UnitTestSuite
{
    [TestClass]
    public class DataChangeTests
    {
        [TestInitialize]
        public void InitializeAppacitive()
        {
            AppInitializer.Initialize();
        }

        [TestMethod]
        public void ChangeGameTilesTest()
        {
            var gameArticles = Articles.FindAllAsync(Schemas.Game, null, new[] {"_id"}, 1, 200).Result;
            string tiles = "g_1,g_3,g_5,b_3,b_5,p_1,p_3,p_5,gr_5,br_1,br_3,br_5,v_1,v_3,v_5,g_1,g_3,b_1,b_5,p_1,p_3,p_5,gr_1,gr_5,br_1,br_3,v_1,v_3,v_5,g_6,b_2,b_6,p_2,p_6,gr_2,gr_4,gr_6,br_2,br_4,br_6,v_2,v_4,v_6,g_4,g_6,b_2,b_4,b_6,p_2,p_4,p_6,gr_2,gr_4,gr_6,br_2,br_4,br_6,v_2,v_4,v_6";
            foreach (var gameArticle in gameArticles)
            {
                gameArticle.Set("tiles", tiles);
                gameArticle.SaveAsync().Wait();
            }
        }

        [TestMethod]
        public void SetGameParametersTest()
        {
            var findQ = BooleanOperator.Or(new[]
            {
                Query.Property("status").IsEqualTo(GameStatus.Waiting.ToString()),
                Query.Property("status").IsEqualTo(GameStatus.Started.ToString()),
            });
            var gameArticles = Articles.FindAllAsync(Schemas.Game, findQ.ToString(), new[] { "_id" }, 1, 200).Result;
            IGameDataProvider gameDataProvider = GameDataProviderFactory.GetGameDataProvider();
            IMovesDataProvider movesDataProvider = MovesDataProviderFactory.GetMovesDataProvider();
            List<string> gameids = gameArticles.Select(g => g.Id).ToList();
            string tiles = "g_1,g_3,g_5,b_3,b_5,p_1,p_3,p_5,gr_5,br_1,br_3,br_5,v_1,v_3,v_5,g_1,g_3,b_1,b_5,p_1,p_3,p_5,gr_1,gr_5,br_1,br_3,v_1,v_3,v_5,g_6,b_2,b_6,p_2,p_6,gr_2,gr_4,gr_6,br_2,br_4,br_6,v_2,v_4,v_6,g_4,g_6,b_2,b_4,b_6,p_2,p_4,p_6,gr_2,gr_4,gr_6,br_2,br_4,br_6,v_2,v_4,v_6";
            do
            {
                var maxIndex = gameids.Count > 20 ? 20 : gameids.Count;
                var ids = gameids.GetRange(0, maxIndex);
                ids = new List<string>() { "39384992024557503" };
                var games = gameDataProvider.GetGameInfo(ids);
                foreach (var game in games)
                {
                    var gameTiles = tiles.Split(',').ToList();
                    foreach (var player in game.Players)
                    {
                        if (player.TilesRemaining > 36)
                        {
                            player.TilesRemaining = 36;
                        }
                        if (player.TilesRemaining >= 6 && player.Tiles.Count < 6)
                        {
                            player.Tiles.AddRange(movesDataProvider.GetRandomTiles(6 - player.Tiles.Count, ref gameTiles));    
                        }
                        gameDataProvider.SaveGamePlayerStatus(game.Id, player);
                    }
                }
                gameids.RemoveRange(0, maxIndex);
            } while (gameids.Count > 0);
        }
    }
}
