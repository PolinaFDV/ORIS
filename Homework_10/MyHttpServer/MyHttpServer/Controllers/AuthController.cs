using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Results;

namespace MyHttpServer.Controllers;

[HttpController("auth")]
public class AuthController : BaseController
{
    [Get("login")]
    public IHttpResult LoginPage()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "static", "login.html");
        if (!File.Exists(path)) return Html("<h1>Файл login.html не найден</h1>");
        return Html(File.ReadAllText(path));
    }

    [Post("login")]
    public IHttpResult Login(
        [FromForm] string login,
        [FromForm] string password,
        [FromQuery] string? source = null)
    {
        const string testLogin = "student";
        const string testPassword = "test123";

        if (login != testLogin || password != testPassword)
        {
            return Json(new
            {
                Success = false,
                Message = "Неверный логин или пароль"
            }, 401);
        }

        int userId = 1;

        string sessionId = SessionService.CreateSession(userId);

        var cookie = new Cookie("sessionId", sessionId, "/")
        {
            HttpOnly = true,
            Secure = Context.Request.IsSecureConnection,
            Expires = DateTime.UtcNow.AddMinutes(30)
        };

        Context.Response.SetCookie(cookie);

        return Json(new
        {
            Success = true,
            Message = "Вход выполнен"
        });
    }

    [Post("logout")]
    public IHttpResult Logout()
    {
        string? sessionId = Context.Request.Cookies["sessionId"]?.Value;

        SessionService.RemoveSession(sessionId);

        var cookie = new Cookie("sessionId", string.Empty, "/")
        {
            HttpOnly = true,
            Secure = Context.Request.IsSecureConnection,
            Expires = DateTime.UtcNow.AddDays(-1)
        };

        Context.Response.SetCookie(cookie);

        return Json(new
        {
            Success = true,
            Message = "Выход выполнен"
        });
    }

    [Get("sendemailtest")]
    public IHttpResult SendEmailTest()
    {
        return Json(new { Success = true, Message = "Test" });
    }

    [Get("json")]
    public IHttpResult JsonPage()
    {
        return Json(new { Username = "user", Age = 13 });
    }
}