// using System;
// using System.Threading.Tasks;
// using Microsoft.Azure.WebJobs;
// using Microsoft.Azure.WebJobs.Extensions.Http;
// using Microsoft.Extensions.Logging;
// using PlayFab.ServerModels;
// using PlayFab.Json;
// using System.Collections.Generic;
// using PlayFab.DataModels;
// using PlayFab.AuthenticationModels;
// using System.Net.Http;
// using System.Net.Http.Headers;
// using Microsoft.AspNetCore.Mvc;
// using System.IO;
// using Newtonsoft.Json;
// using Microsoft.AspNetCore.Http;
// // using PlayFab.Plugins.CloudScript;
// using PlayFab.Samples;
// using SocialEdgeSDK.Server.Constants;
// using PlayFab;
// using System.Net;
// using System.Text;
// using SocialEdgeSDK.Server.Context;
// // namespace SocialEdgeSDK.Playfab
//     public class OnPlayerCreated
//     {
// [FunctionName("ExamplePlaystreamFunc")]

//         public static async Task<dynamic> ExamplePlaystreamFunc(
//             [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
//         {
//             // return "abc";
//             /* Create the function execution's context through the request */
//             var context = JsonConvert.DeserializeObject<PlayerPlayStreamFunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
//             // var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
//             PlayFabSettings.staticSettings.TitleId = context.TitleAuthenticationContext.Id;
//             PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable(Constant.PLAYFAB_DEV_SECRET_KEY, 
//                                                                                                     EnvironmentVariableTarget.Process);
//             // var args = context.FunctionArgument;

//             var playerStatUpdatedEvent = PlayFabSimpleJson.DeserializeObject<dynamic>(context.PlayStreamEventEnvelope.EventData);

           

//             //  var titleDataRequest = new GetTitleDataRequest
//             // {
//             //     Keys = new List<string>{
//             //         Constant.PLAYER_SETTINGS,
//             //         Constant.PLAYER_NAME_NOUNS,
//             //         Constant.PLAYER_NAME_ADJECTIVES
//             //     } 
//             // };

//             // var titleDataResult = await PlayFabServerAPI.GetTitleInternalDataAsync(titleDataRequest);
//             // var res = titleDataResult.Result.Data[Constant.PLAYER_SETTINGS];
//             // var request = new UpdateUserInternalDataRequest
//             // {
//             //     PlayFabId = context.PlayerProfile.PlayerId,//context.CallerEntityProfile.Lineage.MasterPlayerAccountId,//
//             //     Data = new Dictionary<string, string>
//             //     {
//             //         { Constant.PLAYER_SETTINGS, res}
//             //     }
//             // };
//             // /* Use the ApiSettings and AuthenticationContext provided to the function as context for making API calls. */
//             // // var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings, context.AuthenticationContext);

//             // /* Execute the Server API request */
//             // var updateUserDataResponse = await PlayFabServerAPI.UpdateUserInternalDataAsync(request);
//             // var serverApi = new PlayFabServerInstanceAPI(PlayFabSettings, context.AuthenticationContext);

//             var r = new GetEntityTokenRequest
//             {
//                 Entity = new PlayFab.AuthenticationModels.EntityKey{
                    
//                 }
//             };
//             // PlayFabAuthenticationAPI.GetEntityTokenAsync();
//             // var a = new PlayFabDataInstanceAPI()
//             // new PlayFabAuthenticationContext(context.PlayerProfile.)
//             var request = new SetObjectsRequest
//             {
//                 Entity = new PlayFab.DataModels.EntityKey
//                 {
//                     Type = "title_player_account",
//                     Id = context.PlayerProfile.PlayerId
//                 },
//                 Objects = new System.Collections.Generic.List<SetObject>
//                 {
//                     new SetObject{
//                         ObjectName = Constant.PLAYER_SETTINGS,
//                         EscapedDataObject = "something is not good with 100ms limit which i don't like"
//                     }
//                 }
//             };
//             PlayFabAuthenticationContext v= new PlayFabAuthenticationContext(); 
            
//             var setObjectsResult = await PlayFabDataAPI.SetObjectsAsync(request);


//             log.LogInformation($"Unlocked HighSkillContent for {context.PlayerProfile.DisplayName}");

//             return new
//             {
//                 profile = context.PlayerProfile
//             };
//         }
//     }