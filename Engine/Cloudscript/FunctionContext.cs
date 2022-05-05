
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using PlayFab.Samples;
using SocialEdge.Server.Common.Utils;
using SocialEdge.Server.Models;
using SocialEdge.Server.DataService;

namespace SocialEdge.Server.Requests
{
    public class FunctionContext
    {
        private ITitleContext _titleContext;
        private IDataService _dataService;
        private PlayerContext _playerContext;
        private dynamic _args;
        private dynamic _context;

        public PlayerContext FnPlayerContext { get => _playerContext; }
        public dynamic FnArgs { get => _args; }

        public void Base(ITitleContext titleContext, IDataService dataService)
        {
            _titleContext = titleContext;
            _dataService = dataService;
        }

        public async Task FunctionContextInit(HttpRequestMessage req, ILogger log)
        {
            SocialEdgeEnvironment.Init(req, log, _titleContext, _dataService);
            _context = Newtonsoft.Json.JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            _args = _context.FunctionArgument;
            _playerContext = new PlayerContext(_context);
            var maskT = await _playerContext.ValidateCache(FetchBits.NONE);
        }
    }
}