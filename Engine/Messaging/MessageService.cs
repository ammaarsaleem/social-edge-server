/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Requests;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Models;

namespace SocialEdgeSDK.Server.MessageService
{
    public class MessageService : ServerlessHub, IMessageService
    {
        private IDataService _dataService;

        public MessageService(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task Send<T>(string userId, T message)
        {
            await Clients.User(userId).SendAsync("OnMessageRecieved", message);
        }

        public async Task Send<T>(List<string> userIds, T message)
        {
            await Clients.Users(userIds).SendAsync("OnMessageRecieved", message);
        }

        [FunctionName("negotiate")]
        public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req)
        {
            return Negotiate(req.Headers["x-ms-signalr-user-id"]);
        }

        [FunctionName(nameof(OnConnected))]
        public void OnConnected([SignalRTrigger]InvocationContext invocationContext)
        {
            SocialEdge.Init(null, null, _dataService, this);
            SocialEdgePlayerContext socialEdgePlayer = new FunctionContext().LoadPlayer(invocationContext.UserId);
            socialEdgePlayer.PlayerModel.Prefetch(PlayerModelFields.FRIENDS, PlayerModelFields.INFO);
            socialEdgePlayer.PlayerModel.Info.isOnline = true;

            Player.NotifyOnlineStatus(socialEdgePlayer, isOnline : false);
            socialEdgePlayer.CacheFlush();
        }

        [FunctionName(nameof(OnDisconnected))]
        public void OnDisconnected([SignalRTrigger]InvocationContext invocationContext)
        {
            SocialEdge.Init(null, null, _dataService, this);
            SocialEdgePlayerContext socialEdgePlayer = new FunctionContext().LoadPlayer(invocationContext.UserId);
            socialEdgePlayer.PlayerModel.Prefetch(PlayerModelFields.FRIENDS, PlayerModelFields.INFO);
            socialEdgePlayer.PlayerModel.Info.isOnline = false;

            Player.NotifyOnlineStatus(socialEdgePlayer, isOnline : false);
            socialEdgePlayer.CacheFlush();
        }
    }
}
