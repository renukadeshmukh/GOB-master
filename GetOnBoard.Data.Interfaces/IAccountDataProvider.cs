using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GetOnBoard.Core.Model;

namespace GetOnBoard.Data.Interfaces
{
    public interface IAccountDataProvider
    {
        bool AddUser(Account account, out string errorMessage);
        string Login(ref Account account, out string errorMessage);
        List<ChatMessage> GetUserChats(string player1, string player2, int fromDays);
        string SaveMessage(ChatMessage message);
        void UpdateMessageStatus(string messageId);
        List<ChatMessage> GetUnreadMessages(string toMe);
    }
}
