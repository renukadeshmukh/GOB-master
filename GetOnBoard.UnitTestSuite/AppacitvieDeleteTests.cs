using System;
using System.Collections.Generic;
using System.Linq;
using Appacitive.Sdk;
using GetOnBoard.Data.Provider.Appacitive.AppHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GetOnBoard.UnitTestSuite
{
    [TestClass]
    public class AppacitvieDeleteTests
    {
        [TestInitialize]
        public void InitializeAppacitive()
        {
            AppInitializer.Initialize();
        }

        [TestMethod]
        public void DeleteGamePlayerConnectionTest()
        {
            string[] relationType = {"game_player", "game_move", "game_winner"};
            foreach (string relation in relationType)
            {
                var connectionIds = Connections.FindAllAsync(relation,null,new []{"__id"},1, 200, null).Result;
                if(connectionIds.Count > 0)
                    Connections.MultiDeleteAsync(relation, connectionIds.Select(c => c.Id).ToArray()).Wait();
            }
        }

        [TestMethod]
        public void DeleteSchemasTest()
        {
            string[] schemas = { "moves", "game", "log", "exception" };
            foreach (var schema in schemas)
            {
                var articleIds = Articles.FindAllAsync(schema, null, new[] {"__id"}, 1, 200).Result;
                if (articleIds.Count > 0)
                    Articles.MultiDeleteAsync(schema, articleIds.Select( a => a.Id).ToArray()).Wait();
            }
        }
    }
}
