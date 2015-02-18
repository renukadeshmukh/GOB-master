using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GetOnBoard.Core.Factories;
using GetOnBoard.Core.Interfaces;
using GetOnBoard.Data.Factories;
using GetOnBoard.Data.Interfaces;
using GetOnBoard.Services.ServiceImpl.Translators;
using Model = GetOnBoard.Core.Model;
using GetOnBoard.Services.DataContracts;
using GetOnBoard.Services.DataContracts.Messages;
using GetOnBoard.Services.ServiceContracts;

namespace GetOnBoard.Services.ServiceImpl
{
    public class AccountService : IAccountService
    {
        private const string Source = "AccountService";
        public LoginResponse RegisterUser(Account account)
        {
            LoginResponse response = new LoginResponse() {IsSucess = true};
            try
            {
                IAccountDataProvider accountProvider = AccountDataProviderFactory.GetAccountDataProvider();
                string errorMessage;
                if (accountProvider.AddUser(account.ToModel(), out errorMessage) && string.IsNullOrEmpty(errorMessage))
                {
                    response.Account = account;
                }
                else
                {
                    response.ErrorMessage = errorMessage;
                    response.IsSucess = false;
                }
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Registration failed! Please try again.";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex,Source,"RegisterUser", Model.Severity.Critical);
            }
            return response;
        }

        public LoginResponse Login(Account account)
        {
            LoginResponse response = new LoginResponse() { IsSucess = true };
            try
            {
                IAccountDataProvider accountProvider = AccountDataProviderFactory.GetAccountDataProvider();
                string errorMessage;
                Model.Account accountModel = account.ToModel();
                string token = accountProvider.Login(ref accountModel, out errorMessage);
                if (!string.IsNullOrEmpty(token) && string.IsNullOrEmpty(errorMessage))
                {
                    response.Account = accountModel.ToDataContract();
                    response.SessionId = token;
                    ICacheProvider cacheProvider = CacheProviderFactory.GetCacheProvider();
                    cacheProvider.AddValue(token, new Model.UserSession() { Account = accountModel });
                }
                else
                {
                    response.ErrorMessage = errorMessage;
                    response.IsSucess = false;
                }
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Login failed! Please try again.";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "Login", Model.Severity.Critical);
            }
            return response;
        }

        public Response ValidateUserSession(Request request)
        {
            if (request == null || string.IsNullOrEmpty(request.SessionId))
                return new Response() {IsSucess = false, ErrorMessage = "Invalide session request!"};
            Response response = new Response() { IsSucess = true };
            try
            {
                ICacheProvider cache = CacheProviderFactory.GetCacheProvider();
                var session = cache.GetValue(request.SessionId);
                if (session == null)
                {
                    response.IsSucess = false;
                    response.ErrorMessage = "Session expired! Please sign in again.";
                }
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Session expired! Please sign in again.";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "ValidateUserSession", Model.Severity.Critical);
            }
            return response;
        }

        public Response Logout(Request request)
        {
            if (request == null || string.IsNullOrEmpty(request.SessionId))
                return new Response() { IsSucess = false, ErrorMessage = "Invalide logout request!" };
            Response response = new Response() { IsSucess = true };
            try
            {
                ICacheProvider cache = CacheProviderFactory.GetCacheProvider();
                cache.Remove(request.SessionId);
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Logout failed!";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "Logout", Model.Severity.Critical);
            }
            return response;
        }

        public UserChatRs GetUserChats(UserChatRq request)
        {
            if (request == null || string.IsNullOrEmpty(request.SessionId))
                return new UserChatRs() { ErrorMessage = "Invalid GetUserChats request!!", IsSucess = false };
            UserChatRs response = new UserChatRs() { IsSucess = true };
            try
            {
                IAccountDataProvider accountProvider = AccountDataProviderFactory.GetAccountDataProvider();
                var session = GetSession(request.SessionId, response);
                if (session == null)
                    return response;
                response.Chats = accountProvider.GetUserChats(request.Player1, request.Player2, request.FromDays).ToDataContract();
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Failed to pull your games!";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "GetUserGames", Model.Severity.Critical);
            }
            return response;
        }

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

        public SaveMessageRs SaveMessage(SaveMessageRq request)
        {
            SaveMessageRs response = new SaveMessageRs() {IsSucess = true};
            IAccountDataProvider accountProvider = AccountDataProviderFactory.GetAccountDataProvider();
            response.MessageId = accountProvider.SaveMessage(request.Message.ToModel());
            return response;
        }

        public UpdateMessageStatusRs SaveMessageStatus(UpdateMessageStatusRq request)
        {
            UpdateMessageStatusRs response = new UpdateMessageStatusRs() { IsSucess = true };
            IAccountDataProvider accountProvider = AccountDataProviderFactory.GetAccountDataProvider();
            foreach (var messageId in request.MessageIds)
            {
                accountProvider.UpdateMessageStatus(messageId);
            }
            return response;
        }

        public UnreadMessageRs GetUnReadMessages(UnreadMessageRq request)
        {
            if (request == null || string.IsNullOrEmpty(request.SessionId))
                return new UnreadMessageRs() { IsSucess = false, ErrorMessage = "Invalide session request!" };
            UnreadMessageRs response = new UnreadMessageRs() { IsSucess = true };
            try
            {
                var session = GetSession(request.SessionId, response);
                if (session == null)
                {
                    return response;
                }
                Dictionary<string, int> userMessages = new Dictionary<string, int>();
                IAccountDataProvider accountDataProvider = AccountDataProviderFactory.GetAccountDataProvider();
                List<Model.ChatMessage> unreadMessages = accountDataProvider.GetUnreadMessages(session.Account.UserName);
                foreach (var unreadMessage in unreadMessages)
                {
                    if (userMessages.ContainsKey(unreadMessage.From))
                    {
                        userMessages[unreadMessage.From]++;
                    }
                    else
                    {
                        userMessages[unreadMessage.From] = 1;
                    }
                }
                List<UnReadMessageCount> unreadCounts = new List<UnReadMessageCount>();
                foreach (var user in userMessages.Keys)
                {
                    unreadCounts.Add(new UnReadMessageCount() {UserName = user, Count = userMessages[user]});
                }
                response.UnReadMessages = unreadCounts;
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.ErrorMessage = "Session expired! Please sign in again.";
                LoggingDataProviderFactory.GetLoggingDataProvider().LogException(ex, Source, "ValidateUserSession", Model.Severity.Critical);
            }
            return response;
        }
    }
}
