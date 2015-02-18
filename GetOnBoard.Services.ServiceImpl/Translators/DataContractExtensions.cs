using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetOnBoard.Core.Model;
using DC = GetOnBoard.Services.DataContracts;

namespace GetOnBoard.Services.ServiceImpl.Translators
{
    public static class DataContractExtensions
    {
        public static Account ToModel(this DC.Account account)
        {
            if (account == null)
                return null;
            return new Account()
            {
                AccessToken = account.AccessToken,
                Points = account.Points,
                Id = account.Id,
                Level = account.Level,
                UserName = account.UserName,
                LastName = account.LastName,
                FirstName = account.FirstName,
                Email = account.Email,
                MaxGameLimit = account.MaxGameLimit,
                Password = account.Password
            };
        }

        public static ChatMessage ToModel(this DC.ChatMessage message)
        {
            if (message == null)
                return null;
            return new ChatMessage()
            {
                From = message.From,
                To = message.To,
                Message = message.Message,
                Id = message.Id,
                IsSeen = message.IsSeen
            };
        }
    }
}
