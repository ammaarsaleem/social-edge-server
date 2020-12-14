using Newtonsoft.Json;

namespace SocialEdge.Playfab.Photon
{
    public static class Utils
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
    }
}