using System;

namespace MyHttpServer.Framework.Core
{
    public class HttpPutAttribute : RouteAttribute
    {
        public HttpPutAttribute(string path)
            : base("PUT", path)
        {
        }
    }
}