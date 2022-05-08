/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using PlayFab.Samples;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.DataService;

namespace SocialEdgeSDK.Server.Requests
{
    public class FunctionContext
    {
        private ITitleContext _titleContext;
        private IDataService _dataService;
        private SocialEdgePlayer _playerContext;
        private dynamic _args;
        private dynamic _context;

        public SocialEdgePlayer FnPlayerContext { get => _playerContext; }
        public dynamic FnArgs { get => _args; }

        public void Base(ITitleContext titleContext, IDataService dataService = null)
        {
            _titleContext = titleContext;
            _dataService = dataService;
        }

        public async Task FunctionContextInit(HttpRequestMessage req, ILogger log)
        {
            SocialEdge.Init(req, log, _titleContext, _dataService);
            _context = Newtonsoft.Json.JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            _args = _context.FunctionArgument;
            _playerContext = new SocialEdgePlayer(_context);
            var maskT = await _playerContext.ValidateCache(FetchBits.NONE);
        }
    }
}