/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using MongoDB.Bson;
using MongoDB.Bson.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PlayFab.ProfilesModels;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Models;
using PlayFab.Samples;

namespace SocialEdgeSDK.Server.Context
{
    public static class CacheChallengeDataSegments
    {
        public const ulong NONE =               0x0000;
        public const ulong CHALLENGE_MODEL =    0x0001;
        public const ulong MAX = CHALLENGE_MODEL;

        public const ulong READONLY = NONE;
    }

    public class SocialEdgeChallengeContext : ContextCacheBase
    {
        private ChallengeDataModel _challengeModel;

        public ChallengeDataModel ChallengeModel { get => _challengeModel != null ? _challengeModel : _challengeModel = new ChallengeDataModel(this); }

        public SocialEdgeChallengeContext(FunctionExecutionContext<dynamic> context)
        {
            _contextType = ContextType.FUNCTION_EXECUTION_API;
            SocialEdgeChallengeContextInit();
        }

        public SocialEdgeChallengeContext(PlayerPlayStreamFunctionExecutionContext<dynamic> context)
        {
            _contextType = ContextType.PLAYER_PLAYSTREAM;
            SocialEdgeChallengeContextInit();
        }

        private void SocialEdgeChallengeContextInit()
        {
            _fillMap = new Dictionary<ulong, CacheFnType>()
            {
                {CacheChallengeDataSegments.NONE, CacheFillNone},
            };

            _writeMap = new Dictionary<ulong, CacheFnType>()
            {
                {CacheChallengeDataSegments.NONE, CacheWriteNone},
                {CacheChallengeDataSegments.CHALLENGE_MODEL, CacheWriteChallengeModel},
            };
        }

        private bool CacheWriteChallengeModel()
        {
            return _challengeModel != null ? _challengeModel.CacheWrite() : false;
        }
   }
}
