using System;

namespace MyHttpServer.Framework.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RouteAttribute : Attribute
    {
        public string Method { get; }
        public string Path { get; }

        public RouteAttribute(string method, string path)
        {
            Method = method;
            Path = path;
        }
    }
}