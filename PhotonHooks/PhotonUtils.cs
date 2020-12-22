using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
namespace SocialEdge.Playfab.Photon
{
    // TODO: Refactor duplicate code.
    public static class PhotonUtils
    {
        public static bool IsGameValid(GameCreateRequest request, out string message)
        {
            if (string.IsNullOrEmpty(request.GameId))
            {
                message = "Missing GameId.";
                return false;
            }

            if (string.IsNullOrEmpty(request.UserId))
            {
                message = "Missing UserId.";
                return false;
            }

            message = "";
            return true;
        }

        public static bool IsGameValid(GameLeaveRequest request, out string message)
        {
            if (string.IsNullOrEmpty(request.GameId))
            {
                message = "Missing GameId.";
                return false;
            }

            if (string.IsNullOrEmpty(request.UserId))
            {
                message = "Missing UserId.";
                return false;
            }

            message = "";
            return true;
        }

        public static bool IsGameValid(GameCloseRequest request, out string message)
        {
            if (string.IsNullOrEmpty(request.GameId))
            {
                message = "Missing GameId.";
                return false;
            }

            message = "";
            return true;
        }

        public static bool IsGameValid(GamePropertiesRequest request, out string message)
        {
            if (string.IsNullOrEmpty(request.GameId))
            {
                message = "Missing GameId.";
                return false;
            }

            message = "";
            return true;
        }

        public static OkObjectResult GetErrorResponse(string message)
        {
            var errorResponse = new { 
                    ResultCode = 1,
                    Error = message
                };

                return new OkObjectResult(errorResponse);
        }

        public static OkObjectResult GetSuccessResponse()
        {
             var response = new { 
                ResultCode = 0,
                Message = "Success"
            };
            
            return new OkObjectResult(response);
        }

    }
}