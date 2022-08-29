using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SocialEdgeSDK.Server.DataService;

namespace SocialEdgeSDK.Server.Requests
{
    public class Test
    {
        //IDbHelper _dbHelper;
        IDataService _dataService;
        public Test(IDataService dataService)
        {
            _dataService = dataService;
            // _dbHelper = dbHelper;
        }
        /// <summary>
        /// Wild search for a player by namee
        /// </summary>
        /// <param name="name">the name of the user to fetch</param>
        /// <returns>serialiazed json</returns>
        [FunctionName("Test")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string userId = req.Query["userId"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            userId = userId ?? data?.userId;

            string responseMessage = string.IsNullOrEmpty(userId)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {userId}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}

