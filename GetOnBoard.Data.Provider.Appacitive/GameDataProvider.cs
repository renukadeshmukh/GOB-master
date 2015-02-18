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
    public class GameDataProvider : IGameDataProvider
    {
        public GameDataProvider()
        {
            AppInitializer.Initialize();
        }
        
        public List<string> GetUserActiveGames(string userId)
        {
            var user = new User(userId);
            var findQ = BooleanOperator.Or(new[]
            {
                Query.Property("status").IsEqualTo(GameStatus.Waiting.ToString()),
                Query.Property("status").IsEqualTo(GameStatus.Started.ToString()),
                Query.Property("status").IsEqualTo(GameStatus.Invited.ToString())
            });
            var gameArticles = user.GetAllConnectedArticles(Relations.GamePlayer, findQ.ToString(), "game", new [] {"__id"});
            return gameArticles.Select( g => g.Id).ToList();
        }

        public List<string> GetWaitingGamePlayers(List<string> userGames)
        {
            var findQ = Query.Property("status").IsEqualTo(GameStatus.Waiting.ToString());
            List<Article> gameIds = AppacitiveExtensions.GetAllArticles(Schemas.Game, findQ.ToString(), new[] { "__id" });
            gameIds = gameIds.FindAll(w => !userGames.Contains(w.Id)).ToList(); //except user games
            return gameIds.Select(g => g.Id).ToList();
        }

        public bool StartGame(string gameId, string userId, GameStatus status, bool isActive, List<string> tiles, List<string> gameTiles)
        {
            var game = new Article(Schemas.Game, gameId);
            var players = game.GetAllConnectedArticles(Relations.GamePlayer, null, null, new[] {"__id"});
            if (players != null && players.Count == 1)
            {
                game.Set("status", status.ToString());
                game.Set("tiles", string.Join(",", gameTiles));
                var conn = Connection.New(Relations.GamePlayer)
                    .FromExistingArticle("player", userId)
                    .ToExistingArticle("game", gameId);
                conn.Set("ishost", false);
                conn.Set("isactive", isActive);
                conn.Set("tiles", string.Join("|", tiles));
                conn.Set("tiles_remaining", AppConfigurations.MaxTilesPerPlayer);
                conn.SaveAsync().Wait();
                game.SaveAsync().Wait();
                return true;
            }
            return false;
        }

        public Game CreateGame(string userId, GameStatus status, bool isActive, List<string> tiles, List<string> gameTiles)
        {
            var user = new User(userId);
            var game = new Article(Schemas.Game);
            game.Set("status", status.ToString());
            game.Set("tiles", string.Join(",", gameTiles));
            var conn = Connection.New(Relations.GamePlayer)
                .FromExistingArticle("player", userId)
                .ToNewArticle("game", game);
            conn.Set("ishost", true);
            conn.Set("isactive", isActive);
            conn.Set("tiles", string.Join("|", tiles));
            conn.Set("tiles_remaining", AppConfigurations.MaxTilesPerPlayer);
            conn.SaveAsync().Wait();
            return game.ToModelGame();
        }

        public List<Game> GetGameInfo(IEnumerable<string> gameIds)
        {
            List<Game> games = new List<Game>();
            if (gameIds.Any())
            {
                var gameNodes = Graph.Project("get_game_players", gameIds, null).Result;
                if (gameNodes != null)
                {
                    foreach (var gameNode in gameNodes)
                    {
                        Game game = gameNode.Article.ToModelGame();
                        var players = gameNode.GetChildren("player");
                        if (players != null)
                        {
                            DateTime lastActivity = DateTime.MinValue;
                            foreach (var playerNode in players)
                            {
                                User user = playerNode.Article as User;
                                Player player = user.ToModelPlayer(playerNode.Connection);
                                game.Players.Add(player);
                                if (lastActivity <= playerNode.Connection.UtcLastUpdated)
                                {
                                    lastActivity = playerNode.Connection.UtcLastUpdated;
                                }
                            }
                            game.LastActivityTime = lastActivity;
                        }
                        var winner = gameNode.GetChildren("winner").SingleOrDefault();
                        if (winner != null)
                            game.Winner = winner.Article.Id;
                        games.Add(game);
                    }
                }
            }
            return games;
        }

        public bool SaveGamePlayerStatus(string gameId, Player player)
        {
            if (string.IsNullOrWhiteSpace(player.GameConnectionid))
            {
                var gameConnection = Connections.GetAsync(Relations.GamePlayer, gameId, player.Id).Result;
                player.GameConnectionid = gameConnection.Id;
            }
            Connection conn = new Connection(Relations.GamePlayer, player.GameConnectionid);
            conn.Set("points", player.Points);
            conn.Set("tiles", string.Join("|", player.Tiles));
            conn.Set("isactive", player.IsActive);
            conn.Set("tiles_remaining", player.TilesRemaining);
            conn.SaveAsync();
            return true;
        }

        public bool SetPlayerTurn(string gameId, Player player)
        {
            if (string.IsNullOrWhiteSpace(player.GameConnectionid))
            {
                var gameConnection = Connections.GetAsync(Relations.GamePlayer, gameId, player.Id).Result;
                player.GameConnectionid = gameConnection.Id;
            }
            Connection conn = new Connection(Relations.GamePlayer, player.GameConnectionid);
            conn.Set("isactive", player.IsActive);
            conn.SaveAsync();
            return true;
        }


        public bool SaveGameStatus(string gameId, GameStatus status)
        {
            var article = Articles.GetAsync(Schemas.Game, gameId, new[] {"status"}).Result;
            if (!string.Equals(GameStatus.Finished.ToString(), article.Get<string>("status"), StringComparison.OrdinalIgnoreCase))
            {
                var game = new Article(Schemas.Game, gameId);
                game.Set("status", status.ToString());
                game.SaveAsync().Wait();
            }
            return true;
        }

        public bool SetGameWinner(string gameId, string playerId)
        {
            var conn = Connection.New(Relations.GameWinner)
                .FromExistingArticle(Schemas.Game, gameId)
                .ToExistingArticle("winner", playerId);
            conn.SaveAsync().Wait();
            return true;
        }

        public bool UpdatePlayerProfile(string playerId, int points, out int level, out int totalPoints)
        {
            var user = new User(playerId);
            var profileArticle = user.GetConnectedArticlesAsync(Relations.UserProfile).Result.SingleOrDefault();
            totalPoints = profileArticle.Get<int>("total_points");
            totalPoints = totalPoints + points;
            level = (totalPoints/500) + 1;
            profileArticle.Set("total_points", totalPoints);
            profileArticle.Set("level", level);
            profileArticle.SaveAsync().Wait();
            return true;
        }

        public List<Account> GetTopPLayers(int count)
        {
            List<Account> topPlayers = new List<Account>();
            var playersIds = Articles.FindAllAsync(Schemas.Profile, null, new[] {"__id"}, 1, count, "total_points",SortOrder.Descending).Result;
            if (playersIds.Count > 0)
            {
                var projectResult = Graph.Project("get_top_players", playersIds.Select(p => p.Id), null).Result;
                foreach (var profileNode in projectResult)
                {
                    var user = profileNode.GetChildren("user").SingleOrDefault();
                    var account = (user.Article as User).ToModelAccount(profileNode.Article);
                    if (account != null)
                    {
                        topPlayers.Add(account);
                    }
                }
            }
            return topPlayers;
        }

        public List<Game> GetPreviousGames(string userId)
        {
            List<Game> games = new List<Game>();
            var user = new User(userId);
            var findQ = BooleanOperator.Or(new[]
            {
                Query.Property("status").IsEqualTo(GameStatus.Finished.ToString()),
                Query.Property("status").IsEqualTo(GameStatus.Resigned.ToString()),
                Query.Property("status").IsEqualTo(GameStatus.Cancelled.ToString())
            });
            var gameArticles = user.GetAllConnectedArticles(Relations.GamePlayer, findQ.ToString(), "game", new[] { "__id" });
            games = GetGameInfo(gameArticles.Select(g => g.Id));
            return games;
        }

        public Dictionary<string,List<string>> GetGameTiles(List<string> gameIds)
        {
            Dictionary<string, List<string>> gameTilesDict = new Dictionary<string, List<string>>();
            if (gameIds != null && gameIds.Count > 0)
            {
                var ids = new List<string>(gameIds);
                do
                {
                    var maxIndex = ids.Count > 10 ? 10 : ids.Count;
                    var gameArticles = Articles.MultiGetAsync(Schemas.Game, ids.GetRange(0, maxIndex), new[] { "__id", "tiles" }).Result;
                    foreach (var gameArticle in gameArticles)
                    {
                        string tiles = gameArticle.Get<string>("tiles");
                        List<string> gameTiles = string.IsNullOrEmpty(tiles)
                            ? new List<string>()
                            : tiles.Split(',').ToList();
                        gameTilesDict[gameArticle.Id] = gameTiles;
                    }
                    ids.RemoveRange(0, maxIndex);
                } while (ids.Count > 0);
            }
            return gameTilesDict;
        }

        public bool SaveGameTiles(string gameId, List<string> gameTiles)
        {
            var gameArticle = new Article(Schemas.Game, gameId);
            gameArticle.Set("tiles", string.Join(",",gameTiles));
            gameArticle.SaveAsync();
            return true;
        }


        public List<Game> GetUserInvites()
        {
            var findQ = Query.Property("status").IsEqualTo(GameStatus.Invited.ToString());
            var gameArticles = AppacitiveExtensions.GetAllArticles(Schemas.Game, findQ.ToString(), new[] {"__id"});
            var gameInvites = GetGameInfo(gameArticles.Select(g => g.Id));
            return gameInvites;
        }


        
    }
}
