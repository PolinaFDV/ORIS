using System;

namespace MyHttpServer.Framework.Core;

[AttributeUsage(AttributeTargets.Class)]
public class HttpControllerAttribute : Attribute
{
    public string Route { get; }

    public HttpControllerAttribute(string route)
    {
        Route = route.Trim('/');
    }
}
