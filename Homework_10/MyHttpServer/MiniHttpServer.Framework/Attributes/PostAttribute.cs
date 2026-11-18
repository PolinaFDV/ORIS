namespace MyHttpServer.Framework.Core;

public class PostAttribute : RouteAttribute
{
    public PostAttribute(string path)
        : base("POST", path)
    {
    }
}