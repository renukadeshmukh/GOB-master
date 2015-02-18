using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Appacitive.Sdk;
using GetOnBoard.Core.Model;

namespace GetOnBoard.Data.Provider.Appacitive.Extensions
{
    public static class ArticleExtensions
    {
        public static Account ToModelAccount(this User user, Article profile)
        {
            if (user == null)
                return null;
            Account account = new Account()
            {
                UserName = user.Get<string>("username"),
                Email = user.Get<string>("email"),
                FirstName = user.Get<string>("firstname"),
                LastName = user.Get<string>("lastname"),
                Id = user.Id
            };
            if (profile != null)
            {
                account.MaxGameLimit = profile.Get<int>("max_game_limit",0);
                account.Level = profile.Get<int>("level",0);
                account.Points = profile.Get<int>("total_points", 0);
            }
            return account;
        }

        public static Player ToModelPlayer(this User user, Connection gameConnection)
        {
            if (user == null)
                return null;
            Player player = new Player(user.ToModelAccount(null))
            {
                Points = gameConnection.Get<int>("points"), // Game points
                IsActive = gameConnection.Get<bool>("isactive"),
                IsHost = gameConnection.Get<bool>("ishost"),
                Tiles = gameConnection.Get<string>("tiles").Split('|').ToList(),
                TilesRemaining = gameConnection.Get<int>("tiles_remaining"),
                GameConnectionid = gameConnection.Id
            };
            return player;
        }

        public static List<Game> ToModelGames(this List<Article> articles)
        {
            return articles.Select(article => article.ToModelGame()).ToList();
        }

        public static Game ToModelGame(this Article article)
        {
            Game game = new Game();
            game.Id = article.Id;
            game.Status = article.Get<string>("status");
            game.Players = new List<Player>();
            game.LastActivityTime = DateTime.UtcNow;
            return game;
        }

        public static List<Move> ToModelMoves(this List<Article> articles)
        {
            return articles.Select(article => article.ToModelMove()).ToList();
        }

        public static Move ToModelMove(this Article article)
        {
            Move move = new Move()
            {
                MoveCode = article.Get<string>("move_code"),
                Points = article.Get<int>("points"),
                Player = article.Get<string>("player"),
                TimeStamp = article.UtcCreateDate,
                Id = article.Id
            };
            return move;
        }

        public static List<ChatMessage> ToModelChatMessages(this List<Article> articles)
        {
            return articles.Select(article => article.ToModelChatMessage()).ToList();
        }

        public static ChatMessage ToModelChatMessage(this Article article)
        {
            ChatMessage chat = new ChatMessage()
            {
                From = article.Get<string>("from"),
                To = article.Get<string>("to"),
                Message = article.Get<string>("message"),
                IsSeen = article.Get<bool>("is_seen", false),
                TimeStamp = article.UtcCreateDate,
                Id = article.Id
            };
            return chat;
        }
    }
}
