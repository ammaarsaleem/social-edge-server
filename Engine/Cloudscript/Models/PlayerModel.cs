using PlayFab.ServerModels;
using System.Collections.Generic;
namespace SocialEdge.Server.Models
{
    public class PlayerModel
    {
        public GetPlayerCombinedInfoResultPayload combinedInfo;
        public Dictionary<string, PlayFab.ProfilesModels.EntityDataObject> customSettings;

    }
}