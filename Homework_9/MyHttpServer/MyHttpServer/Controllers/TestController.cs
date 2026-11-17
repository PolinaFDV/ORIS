using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Results;

namespace MyHttpServer.Controllers;

[HttpController("test")]
public class TestController : BaseController
{
    [Get("index")]
    public IHttpResult Index()
    {
        return Html("<h1>Hello from controller!</h1>");
    }

    [Get("info")]
    public IHttpResult Info()
    {
        return Json(new
        {
            Id = 1,
            Message = "Hello World!",
            Success = true
        });
    }

    [Get("requestinfo")]
    public IHttpResult RequestInfo()
    {
        return Json(new
        {
            Method = Context.Request.HttpMethod,
            Path = Context.Request.Url?.AbsolutePath ?? string.Empty,
            UserAgent = Context.Request.UserAgent ?? string.Empty
        });
    }

    [Get("hello")]
    public IHttpResult Hello()
    {
        return Html("<p>Hello World!</p>");
    }

    [Post("submit")]
    public IHttpResult Submit()
    {
        return Html("<p>POST request processed</p>");
    }

    [Get("details")]
    public IHttpResult Details([FromQuery] int id, [FromQuery] string name)
    {
        return Html($"<p>ID: {id}, Name: {name}</p>");
    }
}