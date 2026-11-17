using System.Net;
using System.Text;

namespace MyHttpServer.Framework.Results;

public sealed class HtmlResult : IHttpResult
{
    private readonly string _html;
    private readonly int _statusCode;

    public HtmlResult(string html, int statusCode = 200)
    {
        _html = html ?? string.Empty;
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

        byte[] bytes = Encoding.UTF8.GetBytes(_html);

        response.StatusCode = _statusCode;
        response.ContentType = "text/html; charset=utf-8";
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