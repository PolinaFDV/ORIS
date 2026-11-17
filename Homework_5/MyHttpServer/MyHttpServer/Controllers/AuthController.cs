using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using MyHttpServer.Framework;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.HttpResponses;
using MyHttpServer.Services;

namespace MyHttpServer.Controllers;

[HttpController("auth")]
public class AuthController : ControllerBase
{
    [Get("login")]
    public IHttpResponseTypeResult Login()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "static", "login.html");
        if (!File.Exists(path))
        {
            return Html("<h1>Файл login.html не найден</h1>");
        }
        return Html(File.ReadAllText(path));
    }

    [Post("login")]
    public string Login(
        [FromForm] string login,
        [FromForm] string password,
        [FromQuery] string? source = null)
    {
        Console.WriteLine($"Получен логин: {login}. Длина пароля: {password.Length}");

        string backLink = source == "steam" ? "/Steam.html" : "/login.html";

        return $"""
                <!DOCTYPE html>
                <html lang="ru">
                <head><meta charset="utf-8"><title>Результат</title></head>
                <body>
                    <h1>Данные формы получены</h1>
                    <p>Логин: {WebUtility.HtmlEncode(login)}</p>
                    <p>Пароль получен: {password.Length} символов</p>
                    <a href="{backLink}">Вернуться назад</a>
                </body>
                </html>
                """;
    }

    [Get("html")]
    public IHttpResponseTypeResult HtmlPage() => Html("<h1>HTML-ответ работает</h1>");

    [Get("json")]
    public IHttpResponseTypeResult JsonPage() => Json(new { Username = "user", Age = 13 });

    [Post("sendemail")]
    public string SendEmail(
        [FromForm] string demoUser,
        [FromForm] string message)
    {
        Console.WriteLine($"[SendEmail] demoUser: {demoUser}, message: {message}");

        if (string.IsNullOrWhiteSpace(demoUser) || string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Обязательные поля не заполнены.");
        }

        return $"""
                <!DOCTYPE html>
                <html lang="ru">
                <head><meta charset="utf-8"><title>Результат</title></head>
                <body>
                    <h1>Данные для отправки получены</h1>
                    <p>Пользователь: {WebUtility.HtmlEncode(demoUser)}</p>
                    <p>Сообщение: {WebUtility.HtmlEncode(message)}</p>
                    <a href="/Steam.html">Вернуться на страницу Steam</a>
                </body>
                </html>
                """;
    }
}