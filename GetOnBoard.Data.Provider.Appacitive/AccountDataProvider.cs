using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Appacitive.Sdk;
using GetOnBoard.Core.Model;
using GetOnBoard.Data.Interfaces;
using GetOnBoard.Data.Provider.Appacitive.AppHelper;
using GetOnBoard.Data.Provider.Appacitive.Constants;
using GetOnBoard.Data.Provider.Appacitive.Extensions;

namespace GetOnBoard.Data.Provider.Appacitive
{
    public class AccountDataProvider : IAccountDataProvider
    {
        public AccountDataProvider()
        {
            AppInitializer.Initialize();
        }

        public bool AddUser(Account account, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                var exUser = Articles.FindAllAsync(Schemas.User, Query.Property("username").IsEqualTo(account.UserName).ToString(), new[] { "__id" }).Result;
                if (exUser == null || exUser.Count == 0)
                {
                    User user = new User();
                    user.Set("username", account.UserName);
                    user.Set("email", account.Email);
                    user.Set("firstname", account.FirstName);
                    user.Set("lastname", account.LastName);
                    user.Set("password", account.Password);
                    var profile = new Article(Schemas.Profile);
                    profile.Set("total_points", "1");
                    profile.Set("level", "1");
                    profile.Set("max_game_limit", AppConfigurations.MaxAllowedGamesPerUser);
                    var userProfile = Connection.New(Relations.UserProfile)
                        .FromNewArticle(Schemas.User, user)
                        .ToNewArticle(Schemas.Profile, profile).SaveAsync();
                    userProfile.Wait();
                    account.Id = user.Id;
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            errorMessage = "User name already exist!!!";
            return false;
        }


        public string Login(ref Account account, out string errorMessage)
        {
            string token = string.Empty;
            errorMessage = string.Empty;
            var creds = new UsernamePasswordCredentials(account.UserName, account.Password)
            {
                TimeoutInSeconds = 15*60,
                MaxAttempts = int.MaxValue
            };
            try
            {
                // Authenticate
                var result = creds.AuthenticateAsync().Result;
                if (result != null)
                {
                    User loggedInUser = result.LoggedInUser;
                    token = result.UserToken;
                    var profile = loggedInUser.GetConnectedArticlesAsync(Relations.UserProfile).Result;
                    account = loggedInUser.ToModelAccount(profile.SingleOrDefault());
                }
                else
                {
                    errorMessage = "Username and password doesnt match!! Retry.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return token;
        }

        public List<ChatMessage> GetUserChats(string player1, string player2, int fromDays)
        {
            List<ChatMessage> chats = new List<ChatMessage>();
            var fromDate = DateTime.UtcNow.AddDays(-fromDays);
            var findQ = BooleanOperator.And(new[]
            {
                Query.Property("__utcdatecreated").IsGreaterThanEqualToDate(fromDate),
                BooleanOperator.Or(new[]
                {
                    BooleanOperator.And(new []
                    {
                        Query.Property("from").IsEqualTo(player1),
                        Query.Property("to").IsEqualTo(player2)
                    }),
                    BooleanOperator.And(new []
                    {
                        Query.Property("from").IsEqualTo(player2),
                        Query.Property("to").IsEqualTo(player1)
                    }),
                })
            });
            var articles = AppacitiveExtensions.GetAllArticles(Schemas.ChatMessage, findQ.AsString(), null, "__utcdatecreated");
            foreach (var article in articles)
            {
                var chat = article.ToModelChatMessage();
                chats.Add(chat);
            }
            return chats;
        }

        public string SaveMessage(ChatMessage message)
        {
            var article = new Article(Schemas.ChatMessage);
            article.Set("from", message.From);
            article.Set("to", message.To);
            article.Set("message", message.Message);
            article.SaveAsync().Wait();
            return article.Id;
        }

        public void UpdateMessageStatus(string messageId)
        {
            var article = new Article(Schemas.ChatMessage, messageId);
            article.Set("is_seen",true);
            article.SaveAsync();
        }

        public List<ChatMessage> GetUnreadMessages(string toMe)
        {
            List<ChatMessage> chats = new List<ChatMessage>();
            var findQ = BooleanOperator.And(new[]
            {
                Query.Property("to").IsEqualTo(toMe),
                Query.Property("is_seen").IsEqualTo(false)
            });
            var articles = AppacitiveExtensions.GetAllArticles(Schemas.ChatMessage, findQ.AsString(), new [] {"from"}, "__utcdatecreated");
            foreach (var article in articles)
            {
                var chat = article.ToModelChatMessage();
                chats.Add(chat);
            }
            return chats;
        }
    }
}
