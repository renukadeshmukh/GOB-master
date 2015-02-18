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
    [ServiceContract(Name = "IMovesService", Namespace = "http://www.getonboard.com/Services/2012/08")]
    public interface IMovesService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "/GetMyTiles", ResponseFormat = WebMessageFormat.Json)]
        GetMyTilesRs GetMyTiles(GetMyTilesRq request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/GetGameMoves", ResponseFormat = WebMessageFormat.Json)]
        GetGameMovesRs GetGameMoves(GetGameMovesRq request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/PlayMove", ResponseFormat = WebMessageFormat.Json)]
        PlayMoveRs PlayMove(PlayMoveRq request);

        [OperationContract]
        [WebInvoke(UriTemplate = "/PassChance", ResponseFormat = WebMessageFormat.Json)]
        PassChanceRs PassChance(PassChanceRq request);
    }
}
