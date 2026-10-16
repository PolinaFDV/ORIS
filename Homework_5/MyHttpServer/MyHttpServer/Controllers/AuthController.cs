using System;
using System.IO;
using System.Net;
using MyHttpServer.Framework;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.HttpResponses;

namespace MyHttpServer.Controllers;

[HttpController("auth")]
public class AuthController : ControllerBase
{
    [Get("login")]
    public IHttpResponseTypeResult Login()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "static",
            "login.html");

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
        Console.WriteLine(
            $"Получен логин: {login}. Длина пароля: {password.Length}");

        return $"""
                <!DOCTYPE html>
                <html lang="ru">
                <head><meta charset="utf-8"><title>Результат</title></head>
                <body>
                    <h1>Данные формы получены</h1>
                    <p>Логин: {WebUtility.HtmlEncode(login)}</p>
                    <p>Пароль получен: {password.Length} символов</p>
                    <p>Источник: {WebUtility.HtmlEncode(source ?? "не указан")}</p>
                    <a href="/auth/login">Вернуться ко входу</a>
                </body>
                </html>
                """;
    }

    [Get("html")]
    public IHttpResponseTypeResult HtmlPage()
    {
        return Html("<h1>HTML-ответ работает</h1>");
    }

    [Get("json")]
    public IHttpResponseTypeResult JsonPage()
    {
        return Json(new { Username = "user", Age = 13 });
    }

    [Get("json2")]
    public IHttpResponseTypeResult JsonPage2()
    {
        return Json(new { Username = "user", Age = 13 });
    }
}