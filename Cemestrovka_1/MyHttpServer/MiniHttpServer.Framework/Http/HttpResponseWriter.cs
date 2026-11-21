using System.Net;
using System.Text;

namespace MyHttpServer.Framework.Http;

public static class HttpResponseWriter
{
    public static Task WriteHtmlAsync(
        HttpListenerResponse response,
        string html,
        int statusCode = 200)
    {
        return WriteTextAsync(response, html, "text/html; charset=utf-8", statusCode);
    }

    public static async Task WriteTextAsync(
        HttpListenerResponse response,
        string text,
        string contentType,
        int statusCode = 200)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        response.StatusCode = statusCode;
        response.ContentType = contentType;
        response.ContentLength64 = bytes.Length;

        await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);

        try { response.Close(); } catch { }
    }

    public static async Task WriteBytesAsync(
        HttpListenerResponse response,
        byte[] bytes,
        string contentType,
        int statusCode = 200)
    {
        response.StatusCode = statusCode;
        response.ContentType = contentType;
        response.ContentLength64 = bytes.Length;

        await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);

        try { response.Close(); } catch { }
    }
}