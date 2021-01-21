using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
namespace SocialEdge.Server.Db
{
    public interface IDbHelper
    {
        Task<UpdateResult> RegisterPlayer(string playFabId, string name, DateTime loginTime);
        Task<BsonDocument> SearchPlayer(string name);
    }
}