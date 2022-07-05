/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

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
        private dynamic _args;
        private dynamic _context;

        public SocialEdgePlayerContext SocialEdgePlayer { get => _socialEdgePlayer; }
        public dynamic Args { get => _args; }

        public void Base(ITitleContext titleContext, IDataService dataService = null)
        {
            _titleContext = titleContext;
            _dataService = dataService;
        }

        public void InitContext<FunctionContextT>(HttpRequestMessage req, ILogger log)
        {
            SocialEdge.Init(req, log, _titleContext, _dataService);
            log.LogInformation(" SocialEdge.Init done");

            var readT = req.Content.ReadAsStringAsync();
            readT.Wait();
            _context = Newtonsoft.Json.JsonConvert.DeserializeObject<FunctionContextT>(readT.Result);
            _args = _context.FunctionArgument;
            _socialEdgePlayer = new SocialEdgePlayerContext(_context);

            log.LogInformation(" _socialEdgePlayer Init done" + _socialEdgePlayer.ToString());

            _socialEdgePlayer.CacheFill(CacheSegment.NONE);

            log.LogInformation(" _socialEdgePlayer CacheFill called");

        }
    }
}