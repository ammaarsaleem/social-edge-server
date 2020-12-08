using PlayFab.ServerModels;
using System.Collections.Generic;
namespace SocialEdge.Playfab.Models
{
    public class PlayerModel
    {
        public GetPlayerCombinedInfoResultPayload combinedInfo;
         public Dictionary<string, PlayFab.ProfilesModels.EntityDataObject> customSettings;
        // public ;
    }
}