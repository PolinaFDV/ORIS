using System.Net;

namespace MyHttpServer.Framework.HttpResponses;

public interface IHttpResponseTypeResult
{
    byte[] Execute(HttpListenerContext context);
}