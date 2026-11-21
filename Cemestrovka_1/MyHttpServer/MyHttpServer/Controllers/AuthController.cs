using System;
using System.IO;
using System.Net;
using System.Text.Json;
using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Results;

namespace MyHttpServer.Controllers;

[HttpController("auth")]
public class AuthController : BaseController
{
    private static readonly Lazy<Settings> LazySettings = new(() =>
    {
        string settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        return JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsPath))
               ?? throw new InvalidOperationException("Не удалось прочитать settings.json");
    });

    private Settings Settings => LazySettings.Value;

    [Get("login")]
    public IHttpResult LoginPage()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "static", "admin.html");
        if (!File.Exists(path)) return Html("<h1>Файл admin.html не найден</h1>");
        return Html(File.ReadAllText(path));
    }

    [Post("login")]
    public IHttpResult Login(
        [FromForm] string login,
        [FromForm] string password)
    {
        if (login != Settings.AdminLogin || password != Settings.AdminPassword)
        {
            return Json(new { success = false, message = "Неверный логин или пароль" });
        }

        int userId = 1;
        string sessionId = SessionService.CreateSession(userId);

        var cookie = new Cookie("sessionId", sessionId, "/")
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddMinutes(30)
        };

        Context.Response.SetCookie(cookie);
        return Json(new { success = true, message = "Вход выполнен" });
    }

    [Post("logout")]
    public IHttpResult Logout()
    {
        string? sessionId = Context.Request.Cookies["sessionId"]?.Value;
        SessionService.RemoveSession(sessionId);

        var cookie = new Cookie("sessionId", string.Empty, "/")
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(-1)
        };

        Context.Response.SetCookie(cookie);
        return Json(new { success = true, message = "Выход выполнен" });
    }
}