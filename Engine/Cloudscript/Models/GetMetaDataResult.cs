using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
using PlayFab.ProfilesModels;
using PlayFab.DataModels;

namespace SocialEdge.Server.Models{
    public class GetMetaDataResult
    {
        public GetObjectsResponse dataObjects;
        public object liveTournaments;
        public object chat;
        public int inboxCount;
        public object inbox;
        public GetShopResult shop;
        public GetFriendsListResult friends;
        public GetTitleDataResult titleData;
        public  GetEntityProfilesResponse friendsProfiles;

        public bool appVersionValid;
        
    }
}