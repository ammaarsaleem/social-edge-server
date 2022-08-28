/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialEdgeSDK.Server.MessageService
{
    public interface IMessageService
    {
        public Task Send<T>(string playerId, T message);
        public Task Send<T>(List<string> playerIds, T message);
    }
}
