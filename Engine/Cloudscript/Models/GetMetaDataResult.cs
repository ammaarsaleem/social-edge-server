/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
using PlayFab.ServerModels;
using PlayFab.ProfilesModels;

namespace SocialEdgeSDK.Server.Models
{
    public class GetMetaDataResult
    {
        public PlayerDataModel playerDataModel;
        public GetPlayerCombinedInfoResultPayload playerCombinedInfoResultPayload;
        public string publicDataObjs;
        public Dictionary<string, TournamentLiveData> liveTournaments;
        public string chat;
        public int inboxCount;
        public string inbox;
        public GetShopResult shop;
        public List<FriendInfo> friends;
        public GetTitleDataResult titleData;
        public List<EntityProfileBody> friendsProfiles;

        public bool appVersionValid;
        public string contentData;

        
    }
}