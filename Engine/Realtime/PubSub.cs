// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Azure.WebJobs;
// using Microsoft.Azure.WebJobs.Extensions.Http;
// using Microsoft.Azure.WebJobs.Extensions.SignalRService;


// namespace SocialEdge.Server.Realtime
// {
//     public static class PubSub
//     {
//         [FunctionName("negotiate")]
//         public static SignalRConnectionInfo GetToken(
//                             [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
//                             [SignalRConnectionInfo(HubName = "match")] SignalRConnectionInfo connectionInfo)
//         {
//             return connectionInfo;
//         }

//         [FunctionName("AddToGroup")]
//           public static Task AddToGroup(
//                         // [HttpTrigger(AuthorizationLevel.Anonymous, "AddToGroup")] HttpRequest req,
//                         [SignalR(HubName ="match")]IAsyncCollector<SignalRGroupAction> signalRGroupActions,
//                         string userId, string groupId
//         )
//         {
//             return signalRGroupActions.AddAsync(
//                  new SignalRGroupAction
//                  {
//                      UserId = userId,
//                      GroupName = groupId,
//                      Action = GroupAction.Add
//                  });
//         }

//         [FunctionName("RemoveFromGroup")]
//         public static Task RemoveFromGroup(
//                         [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
//                         [SignalR(HubName ="match")]IAsyncCollector<SignalRGroupAction> signalRGroupActions)
//         {
//             return signalRGroupActions.AddAsync(
//                  new SignalRGroupAction
//                  {
//                      UserId = userId,
//                      GroupName = groupId,
//                      Action = GroupAction.Remove
//                  });
//         }

//         [FunctionName("RemoveAll")]
//         public static Task RemoveAll(
//                         [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
//                         [SignalR(HubName ="match")]IAsyncCollector<SignalRGroupAction> signalRGroupActions)
//         {
//             return signalRGroupActions.AddAsync(
//                  new SignalRGroupAction
//                  {
//                      GroupName = groupId,
//                      Action = GroupAction.RemoveAll
//                  });            
//         }

//         [FunctionName("SendToUser")]
//         public static Task SendToUser(
//                             [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
//                             [SignalR(HubName = "match")] IAsyncCollector<SignalRMessage> signalRMessages )
//         {
//             return signalRMessages.AddAsync(
//                 new SignalRMessage
//                 {
//                     UserId = userId,
//                     Target = method,
//                     Arguments = new[] { args }
//                 });            
//         }

//         [FunctionName("SendToGroup")]
//         public static Task SendToGroup(
//                             [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
//                             [SignalR(HubName = "match")] IAsyncCollector<SignalRMessage> signalRMessages)
//         {
//             return signalRMessages.AddAsync(
//                 new SignalRMessage
//                 {
//                     GroupName = groupId,
//                     Target = method,
//                     Arguments = new[] { args }
//                 });
//         }

//         [FunctionName("Broadcast")]
//         public static Task Broadcast(
//                         [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
//                         [SignalR(HubName = "match")] IAsyncCollector<SignalRMessage> signalRMessages)
//         {
//             return signalRMessages.AddAsync(
//                 new SignalRMessage
//                 {
//                     Target = method,
//                     Arguments = new[] { args }
//                 });
//         }     
//     }
// }
