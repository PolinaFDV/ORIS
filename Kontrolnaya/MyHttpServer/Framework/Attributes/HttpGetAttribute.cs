namespace MyHttpServer.Framework.Core;

public class HttpGetAttribute : RouteAttribute
{
    public HttpGetAttribute(string path)
        : base("GET", path)
    {
    }
}