using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using GetOnBoard.Services.DataContracts;
using GetOnBoard.Services.DataContracts.Messages;

namespace GetOnBoard.Services.ServiceContracts
{
    [ServiceContract(Name = "IAccountService", Namespace = "http://www.getonboard.com/Services/2012/08")]
    public interface IAccountService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "/RegisterUser", ResponseFormat = WebMessageFormat.Json)]
        LoginResponse RegisterUser(Account account);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Login", ResponseFormat = WebMessageFormat.Json)]
        LoginResponse Login(Account account);

        [OperationContract]
        [WebInvoke(UriTemplate = "/ValidateSession", ResponseFormat = WebMessageFormat.Json)]
        Response ValidateUserSession(Request request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Logout", ResponseFormat = WebMessageFormat.Json)]
        Response Logout(Request request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/GetMyChats", ResponseFormat = WebMessageFormat.Json)]
        UserChatRs GetUserChats(UserChatRq request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/SaveMessage", ResponseFormat = WebMessageFormat.Json)]
        SaveMessageRs SaveMessage(SaveMessageRq request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/SaveMessageStatus", ResponseFormat = WebMessageFormat.Json)]
        UpdateMessageStatusRs SaveMessageStatus(UpdateMessageStatusRq request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/GetUnReadMessages", ResponseFormat = WebMessageFormat.Json)]
        UnreadMessageRs GetUnReadMessages(UnreadMessageRq request);
    }
}
