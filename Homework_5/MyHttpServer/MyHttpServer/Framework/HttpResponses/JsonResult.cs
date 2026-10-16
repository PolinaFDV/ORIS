using System.Net;
using System.Text;
using System.Text.Json;

namespace MyHttpServer.Framework.HttpResponses;

internal class JsonResult : IHttpResponseTypeResult
{
    private readonly object _data;

    public JsonResult(object data)
    {
        _data = data;
    }

    public byte[] Execute(HttpListenerContext context)
    {
        string json = JsonSerializer.Serialize(_data);
        context.Response.ContentType = "application/json; charset=utf-8";

        return Encoding.UTF8.GetBytes(json);
    }
}