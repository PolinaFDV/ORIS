namespace MyHttpServer.Framework.Core;

public class HttpPostAttribute : RouteAttribute
{
    public HttpPostAttribute(string path)
        : base("POST", path)
    {
    }
}