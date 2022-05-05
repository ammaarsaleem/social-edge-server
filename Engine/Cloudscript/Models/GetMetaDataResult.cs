using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
using PlayFab.ProfilesModels;
using PlayFab.DataModels;
using MongoDB.Bson;

namespace SocialEdge.Server.Models{
    public class GetMetaDataResult
    {
        public string publicDataObjs;
        public string liveTournaments;
        public string chat;
        public int inboxCount;
        public string inbox;
        public GetShopResult shop;
        public List<FriendInfo> friends;
        public GetTitleDataResult titleData;
        public List<EntityProfileBody> friendsProfiles;

        public bool appVersionValid;
        
    }
}