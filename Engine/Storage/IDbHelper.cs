using System;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace SocialEdge.Server.Db
{
    public interface IDbHelper
    {
        Task<bool> RegisterPlayer(string playFabId, string name, DateTime loginTime);
        Task<Dictionary<string,object>> SearchPlayerByName(string name);
    }
}