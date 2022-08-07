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
    public static class CacheTournamentDataSegments
    {
        #pragma warning disable format 
        public const ulong NONE =               0x0000;
        public const ulong TOURNAMENT_ENTRY =   0x0001;
        public const ulong TOURNAMENT_LIVE  =   0x0002;
        public const ulong TOURNAMENT_MODEL =   0x0004;
        #pragma warning restore format

        public const ulong MAX = TOURNAMENT_MODEL;

        public const ulong META = TOURNAMENT_LIVE;
        public const ulong READONLY = TOURNAMENT_ENTRY;
    }

    public class SocialEdgeTournamentContext : ContextCacheBase
    {
        private TournamentLiveModel _tournamentLiveModel;
        private TournamentEntryModel _tournamentEntryModel;
        private TournamentDataModel _tournamentModel;

        public TournamentDataModel TournamentModel { get => _tournamentModel != null ? _tournamentModel : _tournamentModel = new TournamentDataModel(this); }
        public TournamentLiveModel TournamentLiveModel { get => _tournamentLiveModel != null ? _tournamentLiveModel : _tournamentLiveModel = new TournamentLiveModel(); }
        public TournamentEntryModel TournamentEntryModel { get => _tournamentEntryModel != null ? _tournamentEntryModel : _tournamentEntryModel = new TournamentEntryModel(this); }

        public SocialEdgeTournamentContext(FunctionExecutionContext<dynamic> context)
        {
            _contextType = ContextType.FUNCTION_EXECUTION_API;
            SocialEdgeTournamentContextInit();
        }

        public SocialEdgeTournamentContext(PlayerPlayStreamFunctionExecutionContext<dynamic> context)
        {
            _contextType = ContextType.PLAYER_PLAYSTREAM;
            SocialEdgeTournamentContextInit();
        }

        private void SocialEdgeTournamentContextInit()
        {
            _fillMap = new Dictionary<ulong, CacheFnType>()
            {
                {CacheTournamentDataSegments.NONE, CacheFillNone},
            };

            _writeMap = new Dictionary<ulong, CacheFnType>()
            {
                {CacheTournamentDataSegments.NONE, CacheWriteNone},
                {CacheTournamentDataSegments.TOURNAMENT_ENTRY, CacheWriteTournamentEntryModel},
                {CacheTournamentDataSegments.TOURNAMENT_LIVE, CacheWriteReadOnlyError},
                {CacheTournamentDataSegments.TOURNAMENT_MODEL, CacheWriteTournamentModel}
            };
        }

        private bool CacheWriteTournamentEntryModel()
        {
            return _tournamentEntryModel != null ? _tournamentEntryModel.CacheWrite() : false;
        }

        private bool CacheWriteTournamentModel()
        {
            return _tournamentModel != null ? _tournamentModel.CacheWriteTournamentModel() : false;
        }
   }
}