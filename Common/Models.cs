namespace SocialEdge.Server.Common.Models{
    public class Result
    {
        public bool isSuccess = false;
        public string error = string.Empty;
        public Result(bool success, string err)
        {
            isSuccess = success;
            error = err;
        }
    }
}