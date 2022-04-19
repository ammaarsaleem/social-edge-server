using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
using PlayFab.ProfilesModels;

namespace SocialEdge.Server.Models{
    public class GetMetaDataResult
    {
        public object inbox;
        public GetShopResult shop;
        public GetFriendsListResult friends;
        public GetTitleDataResult titleData;

        public  GetEntityProfilesResponse friendsProfiles;

        public bool appVersionValid;
        
    }
}