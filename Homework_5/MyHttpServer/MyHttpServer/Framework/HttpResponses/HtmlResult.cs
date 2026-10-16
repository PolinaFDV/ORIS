using System.Net;
using System.Text;

namespace MyHttpServer.Framework.HttpResponses;

internal class HtmlResult : IHttpResponseTypeResult
{
    private readonly string _html;

    public HtmlResult(string html)
    {
        _html = html;
    }

    public byte[] Execute(HttpListenerContext context)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        return Encoding.UTF8.GetBytes(_html);
    }
}