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

namespace SocialEdgeSDK.Server.MessageService
{
    public class MessageService : ServerlessHub, IMessageService
    {
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
    }
}
