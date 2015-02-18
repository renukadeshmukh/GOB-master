using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetOnBoard.Services.DataContracts;
using Model = GetOnBoard.Core.Model;

namespace GetOnBoard.Services.ServiceImpl.Translators
{
    public static class ModelExtensions
    {
        public static List<Account> ToDataContract(this List<Model.Account> accounts)
        {
            return accounts.Select(g => g.ToDataContract()).ToList();
        }

        public static Account ToDataContract(this Model.Account account)
        {
            if(account==null)
                return null;
            return new Account()
            {
                AccessToken = account.AccessToken,
                UserName = account.UserName,
                Id = account.Id,
                Points = account.Points,
                LastName = account.LastName,
                FirstName = account.FirstName,
                Email = account.Email,
                Level = account.Level,
                MaxGameLimit = account.MaxGameLimit,
            };
        }

        public static Player ToDataContract(this Model.Player player, bool passTiles=true)
        {
            if (player == null)
                return null;
            Player playerC = new Player()
            {
                Points = player.Points,
                Id = player.Id,
                UserName = player.UserName,
                Level = player.Level,
                IsActive = player.IsActive,
                IsHost = player.IsHost,
                TilesRemaining = player.TilesRemaining,
            };
            if (passTiles)
            {
                playerC.Tiles = player.Tiles;
            }
            return playerC;
        }

        public static List<Game> ToDataContract(this List<Model.Game> games, string myUserId)
        {
            return games.Select(g => g.ToDataContract(myUserId)).ToList();
        }

        public static Game ToDataContract(this Model.Game game, string myUserId)
        {
            if (game == null)
                return null;
            Game g = new Game()
            {
                Id = game.Id,
                Status = game.Status,
                Winner = game.Winner,
            };
            g.LastActivityTime = GetLastActivityTime(game.LastActivityTime);
            g.Players = new List<Player>();
            foreach (var player in game.Players)
            {
                bool passTilesInfo = string.Equals(player.Id, myUserId);
                g.Players.Add(player.ToDataContract(passTilesInfo));
            }
            return g;
        }

        public static List<Move> ToDataContract(this List<Model.Move> moves)
        {
            return moves.Select(g => g.ToDataContract()).ToList();
        }

        public static Move ToDataContract(this Model.Move move)
        {
            if (move == null)
                return null;
            return new Move()
            {
                Id = move.Id,
                Points = move.Points,
                Player = move.Player,
                MoveCode = move.MoveCode
            };
        }

        public static List<ChatMessage> ToDataContract(this List<Model.ChatMessage> chats)
        {
            return chats.Select(g => g.ToDataContract()).ToList();
        }

        public static ChatMessage ToDataContract(this Model.ChatMessage chat)
        {
            if (chat == null)
                return null;
            return new ChatMessage()
            {
                Id = chat.Id,
                IsSeen = chat.IsSeen,
                From = chat.From,
                To = chat.To,
                Message = chat.Message,
                TimeStamp = string.Format("{0:s}", chat.TimeStamp)
            };
        }

        private static string GetLastActivityTime(DateTime lastActivity)
        {
            string lastActivityTime = string.Empty;
            var time = DateTime.UtcNow - lastActivity;
            if (time.TotalDays >= 1)
                lastActivityTime = Math.Round(time.TotalDays) + " days ago";
            else if (time.TotalHours >= 1)
                lastActivityTime = Math.Round(time.TotalHours) + " hours ago";
            else if (time.TotalMinutes >= 1)
                lastActivityTime = Math.Round(time.TotalMinutes) + " minutes ago";
            else
                lastActivityTime = Math.Round(time.TotalSeconds < 0 ? 0 : time.TotalSeconds) + " seconds ago";
            return lastActivityTime;
        }
    }
}
