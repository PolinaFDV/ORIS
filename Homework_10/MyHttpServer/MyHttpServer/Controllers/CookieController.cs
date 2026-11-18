using System.Net;
using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Results;

namespace MyHttpServer.Controllers;

[HttpController("cookie")]
public class CookieController : BaseController
{
    [Get("set")]
    public IHttpResult Set()
    {
        var cookie = new Cookie(
            "testCookie",
            "HelloWorld",
            "/")
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddMinutes(30)
        };

        Context.Response.SetCookie(cookie);

        return Html("<h1>Cookie установлена</h1>");
    }

    [Get("get")]
    public IHttpResult Get()
    {
        string? value =
            Context.Request.Cookies["testCookie"]?.Value;

        return Json(new
        {
            Value = value
        });
    }

    [Get("delete")]
    public IHttpResult Delete()
    {
        var cookie = new Cookie(
            "testCookie",
            string.Empty,
            "/")
        {
            Expires = DateTime.UtcNow.AddDays(-1)
        };

        Context.Response.SetCookie(cookie);

        return Html("<h1>Cookie удалена</h1>");
    }
}