namespace SocialEdge.Server.DataService
{
    public interface IDataService
    {
        ICollection GetCollection(string name);
        //ICache GetCache();
    }
}