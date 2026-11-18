using System.Net;
using MyHttpServer.Framework.Authentication;
using MyHttpServer.Framework.Results;

namespace MyHttpServer.Framework.Controllers;

public abstract class BaseController
{
    protected HttpListenerContext Context { get; private set; } = null!;
    
    protected IUserSessionService SessionService { get; private set; } = null!;

    public void Initialize(HttpListenerContext context, IUserSessionService sessionService)
    {
        Context = context;
        SessionService = sessionService;
    }

    protected IHttpResult Html(string html, int statusCode = 200)
    {
        return new HtmlResult(html, statusCode);
    }

    protected IHttpResult Json(object data, int statusCode = 200)
    {
        return new JsonResult(data, statusCode);
    }

    protected IHttpResult Page(string templatePath, object model, int statusCode = 200)
    {
        return new PageResult(templatePath, model, statusCode);
    }
}