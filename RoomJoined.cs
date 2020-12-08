// using System;
// using System.Threading.Tasks;
// using Microsoft.Azure.WebJobs;
// using Microsoft.Azure.WebJobs.Extensions.Http;
// using Microsoft.Extensions.Logging;
// using PlayFab.ServerModels;
// using PlayFab.Json;
// using System.Collections.Generic;
// using PlayFab.DataModels;
// using System.Net.Http;
// using System.Net.Http.Headers;
// using Microsoft.AspNetCore.Mvc;
// using System.IO;
// using Newtonsoft.Json;
// using Microsoft.AspNetCore.Http;
// // using PlayFab.Plugins.CloudScript;
// using PlayFab.Samples;
// using SocialEdge.Server.Constants;
// using PlayFab;

// namespace SocialEdge.Playfab
// {
//     public class RoomJoined
//     {
//         [FunctionName("RoomJoined")]
//         public async Task<HttpResponseMessage> Run(
//             [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "{appId}/RoomJoined/{id:int?}")] 
//             HttpRequestMessage req, string appId)
//         {
//            log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

//             // Get request body
//             GameLeaveRequest body = await req.Content.ReadAsAsync<GameLeaveRequest>();

//             var okMsg = $"{req.RequestUri} - Recieved Game Join Request";
//             log.Info(okMsg);
//             return req.CreateResponse(HttpStatusCode.OK, okMsg);
//         }
//     }
// }

