using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Http;

internal static class HttpResponseWriter
{
    public static Task WriteHtmlAsync(
        HttpListenerResponse response,
        string html,
        int statusCode = 200)
    {
        return WriteAsync(
            response,
            html,
            "text/html; charset=utf-8",
            statusCode);
    }

    public static async Task WriteAsync(
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
        response.Close();
    }

    public static async Task WriteBytesAsync(
        HttpListenerResponse response,
        byte[] bytes,
        int statusCode = 200)
    {
        response.StatusCode = statusCode;
        response.ContentLength64 = bytes.Length;

        await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        response.Close();
    }
}