using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using GetOnBoard.Services.DataContracts.Messages;

namespace GetOnBoard.Services.ServiceContracts
{
    [ServiceContract(Name = "IGameService", Namespace = "http://www.getonboard.com/Services/2012/08")]
    public interface IGameService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "/StartGame", ResponseFormat = WebMessageFormat.Json)]
        CreateGameRs CreateGame(CreateGameRq request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/GetMyGames", ResponseFormat = WebMessageFormat.Json)]
        UserGamesRs GetUserGames(UserGamesRq request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/GetGameStatus", ResponseFormat = WebMessageFormat.Json)]
        GameStatusRs GetGameStatus(GameStatusRq request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/GetTopPlayers", ResponseFormat = WebMessageFormat.Json)]
        GetTopPlayersRs GetTopPlayers(GetTopPlayersRq request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/DeleteGame", ResponseFormat = WebMessageFormat.Json)]
        DeleteGameRs DeleteGame(DeleteGameRq request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/ResignGame", ResponseFormat = WebMessageFormat.Json)]
        ResignGameRs ResignGame(ResignGameRq request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/GetPreviousGames", ResponseFormat = WebMessageFormat.Json)]
        UserGamesRs GetPreviousGames(UserGamesRq request);
    }
}
