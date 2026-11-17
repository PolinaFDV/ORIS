namespace MyHttpServer.Framework.Core;

public class GetAttribute : RouteAttribute
{
    public GetAttribute(string path)
        : base("GET", path)
    {
    }
}