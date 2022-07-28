/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.DataService;

namespace SocialEdgeSDK.Server.Requests
{
    public class FunctionContext
    {
        private ITitleContext _titleContext;
        private IDataService _dataService;
        private SocialEdgePlayerContext _socialEdgePlayer;
        private SocialEdgeTournamentContext _socialEdgeTournament;
        private SocialEdgeChallengeContext _socialEdgeChallenge;
        private dynamic _args;
        private dynamic _context;

        public delegate bool CacheFlushFn();
        public List<CacheFlushFn> _cacheFlush;

        public SocialEdgePlayerContext SocialEdgePlayer { get => _socialEdgePlayer; }
        public SocialEdgeTournamentContext SocialEdgeTournament { get => _socialEdgeTournament; }
        public SocialEdgeChallengeContext SocialEdgeChallenge { get => _socialEdgeChallenge; }
        public dynamic Args { get => _args; }

        public void Base(ITitleContext titleContext, IDataService dataService = null)
        {
            _titleContext = titleContext;
            _dataService = dataService;
            _cacheFlush = new List<CacheFlushFn>();
        }

        public void InitContext<FunctionContextT>(HttpRequestMessage req, ILogger log)
        {
            SocialEdge.Init(req, log, _titleContext, _dataService);
            var readT = req.Content.ReadAsStringAsync();
            readT.Wait();
            _context = Newtonsoft.Json.JsonConvert.DeserializeObject<FunctionContextT>(readT.Result);
            _args = _context.FunctionArgument;
            _socialEdgePlayer = new SocialEdgePlayerContext(_context);
            _socialEdgePlayer.CacheFill(CachePlayerDataSegments.NONE);
            _socialEdgeTournament = new SocialEdgeTournamentContext(_context);
            _socialEdgeChallenge = new SocialEdgeChallengeContext(_context);

            RegisterCacheFlush(SocialEdgePlayer.CacheFlush);
            RegisterCacheFlush(SocialEdgeTournament.CacheFlush);
            RegisterCacheFlush(SocialEdgeChallenge.CacheFlush);
        }

        public void RegisterCacheFlush(CacheFlushFn fn)
        {
            _cacheFlush.Add(fn);
        }

        public SocialEdgePlayerContext LoadPlayer(string playerId)
        {
            SocialEdgePlayerContext player = new SocialEdgePlayerContext(playerId);
            RegisterCacheFlush(player.CacheFlush);
            return player;        
        }

        public void CacheFlush()
        {
            foreach(var cacheFlushFn in _cacheFlush)
                cacheFlushFn.Invoke();
        }
    }
}