using System.Net;
using MyHttpServer.Framework.HttpResponses;

namespace MyHttpServer.Framework;

public class ControllerBase
{
    protected HttpListenerContext Context { get; private set; } = null!;

    internal void SetContext(HttpListenerContext context)
    {
        Context = context;
    }

    protected IHttpResponseTypeResult Html(string html)
    {
        return new HtmlResult(html);
    }

    protected IHttpResponseTypeResult Json(object data)
    {
        return new JsonResult(data);
    }
}