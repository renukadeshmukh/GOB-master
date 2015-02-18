using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GetOnBoard.BL;
using GetOnBoard.Services.DataContracts;
using GetOnBoard.Services.DataContracts.Messages;
using Microsoft.AspNet.SignalR;

namespace GetOnBoard.Hubs
{
    public class GameHub : Hub
    {
        public override Task OnConnected()
        {
            try
            {
                string userName = Context.Request.QueryString["userName"];
                UserDataBase.SaveUserConnection(userName, Context.ConnectionId);
                Clients.Others.UpdateUserStatus(userName, true);
            }
            catch (Exception ex){}
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            try
            {
                string userName = Context.Request.QueryString["userName"];
                string connectionId = UserDataBase.GetUserConnectionId(userName);
                if (string.Equals(connectionId, Context.ConnectionId))
                {
                    UserDataBase.RemoveUserConnectionId(userName);
                    Clients.Others.UpdateUserStatus(userName, false);
                }
            }
            catch (Exception ex){}
            return base.OnDisconnected();
        }

        public void GetPlayerStatus(List<string> myPlayers)
        {
            try
            {
                if (myPlayers != null)
                {
                    List<PlayerStatus> players = new List<PlayerStatus>();
                    foreach (var myPlayer in myPlayers)
                    {
                        players.Add(new PlayerStatus() {UserName = myPlayer, IsOnline = UserDataBase.IsOnline(myPlayer)});
                    }
                    Clients.Client(Context.ConnectionId).SetPlayerStatus(players);
                }
            }
            catch (Exception ex){}
        }

        public void SendMessage(string to, string message)
        {
            try
            {
                string from = Context.Request.QueryString["userName"];
                string connectionId = UserDataBase.GetUserConnectionId(to);
                ChatMessage chatMessage = new ChatMessage()
                {
                    From = from,
                    To = to,
                    Message = message,
                    IsSeen = false,
                    TimeStamp = string.Format("{0:s}", DateTime.Now)
                };
                string url = string.Format("{0}/get/AccountService.svc/SaveMessage", Configs.BaseSiteUrl);
                HttpClient client = new HttpClient(url, ContentType.Json);
                SaveMessageRs response =
                    client.Post<SaveMessageRq, SaveMessageRs>(new SaveMessageRq() {Message = chatMessage});
                chatMessage.Id = response.MessageId;
                if (!string.IsNullOrEmpty(connectionId))
                {
                    Clients.Client(connectionId).RenderMessage(from, chatMessage);
                }
                Clients.Client(Context.ConnectionId).RenderMessage(to, chatMessage);
                    // Send message to sender also as confirmation.
            }
            catch (Exception ex) { }
        }

        public void SendMessageStatus(string to, List<string> messageIds)
        {
            try
            {
                string from = Context.Request.QueryString["userName"];
                string connectionId = UserDataBase.GetUserConnectionId(to);
                if (!string.IsNullOrEmpty(connectionId))
                {
                    Clients.Client(connectionId).RenderMessageStatus(from, messageIds);
                }
                string url = string.Format("{0}/get/AccountService.svc/SaveMessageStatus", Configs.BaseSiteUrl);
                HttpClient client = new HttpClient(url, ContentType.Json);
                UpdateMessageStatusRs response =
                    client.Post<UpdateMessageStatusRq, UpdateMessageStatusRs>(new UpdateMessageStatusRq()
                    {
                        MessageIds = messageIds
                    });
            }
            catch (Exception ex) { }
        }

        public void NotifyTurn(string to, string gameId)
        {
            try
            {
                string from = Context.Request.QueryString["userName"];
                string connectionId = UserDataBase.GetUserConnectionId(to);
                if (!string.IsNullOrEmpty(connectionId))
                {
                    Clients.Client(connectionId).ItsMyTurn(from, gameId);
                }
            }
            catch (Exception ex) { }
        }
    }
}