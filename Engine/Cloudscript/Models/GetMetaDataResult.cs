using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
namespace SocialEdge.Server.Models{
    public class GetMetaDataResult
    {
        public GetShopResult shop;
        public GetFriendsListResult friends;
    }
}