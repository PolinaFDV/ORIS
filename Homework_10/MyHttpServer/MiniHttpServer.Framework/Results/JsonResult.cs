using System.Net;
using System.Text;
using System.Text.Json;

namespace MyHttpServer.Framework.Results;

public sealed class JsonResult : IHttpResult
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly object _data;
    private readonly int _statusCode;

    public JsonResult(object data, int statusCode = 200)
    {
        _data = data;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(
        HttpListenerResponse response,
        CancellationToken cancellationToken = default)
    {
        if (response.OutputStream == null || !response.OutputStream.CanWrite)
        {
            return;
        }

        string json = JsonSerializer.Serialize(_data, Options);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        response.StatusCode = _statusCode;
        response.ContentType = "application/json; charset=utf-8";
        response.ContentLength64 = bytes.Length;

        await response.OutputStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);

        try
        {
            response.Close();
        }
        catch
        {
        }
    }
}