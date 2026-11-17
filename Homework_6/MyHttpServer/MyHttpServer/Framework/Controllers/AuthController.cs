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
    public IHttpResult Login()
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
    public IHttpResult Login(
        [FromForm] string login,
        [FromForm] string password,
        [FromQuery] string? source = null)
    {
        Console.WriteLine($"Получен логин: {login}. Длина пароля: {password.Length}");

        string backLink = source == "steam" ? "/Steam.html" : "/login.html";

        return Html($"""
                     <!DOCTYPE html>
                     <html lang="ru">
                     <head>
                         <meta charset="utf-8">
                         <title>Результат</title>
                         <link rel="stylesheet" href="/css/style.css">
                     </head>
                     <body>
                         <h1>Данные формы получены</h1>
                         <p>Логин: {WebUtility.HtmlEncode(login)}</p>
                         <p>Пароль получен: {password.Length} символов</p>
                         <a href="{backLink}">Вернуться на главную</a>
                     </body>
                     </html>
                     """);
    }

    [Get("html")]
    public IHttpResult HtmlPage()
    {
        return Html("<h1>HTML-ответ работает</h1>");
    }

    [Get("json")]
    public IHttpResult JsonPage()
    {
        return Json(new { Username = "user", Age = 13 });
    }

    [Post("sendemail")]
    public IHttpResult SendEmail(
        [FromForm] string demoUser,
        [FromForm] string message)
    {
        if (string.IsNullOrWhiteSpace(demoUser) || string.IsNullOrWhiteSpace(message))
        {
            return Json(new
            {
                Success = false,
                Message = "Обязательные поля не заполнены"
            }, 400);
        }

        try
        {
            EmailSettings settings = LoadEmailSettings();

            MailAddress from = new MailAddress(settings.SenderEmail, settings.SenderName);
            MailAddress to = new MailAddress(settings.SenderEmail);

            using MailMessage mail = new MailMessage(from, to);
            mail.Subject = "Тестовое сообщение из MiniHttpServer";
            mail.Body = $"Пользователь: {demoUser}\nСообщение: {message}";
            mail.IsBodyHtml = false;

            using SmtpClient smtp = new SmtpClient(settings.Host, settings.Port);
            smtp.Credentials = new NetworkCredential(settings.SenderEmail, settings.Password);
            smtp.EnableSsl = true;

            smtp.Send(mail);

            return Json(new
            {
                Success = true,
                Message = "Сообщение отправлено"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SendEmail ERROR] {ex.GetBaseException().Message}");

            return Json(new
            {
                Success = false,
                Message = "Не удалось отправить сообщение"
            }, 500);
        }
    }

    private static EmailSettings LoadEmailSettings()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "settings.json");

        if (!File.Exists(path))
        {
            return new EmailSettings();
        }

        string json = File.ReadAllText(path);
        Settings? settings = JsonSerializer.Deserialize<Settings>(json);

        return settings?.Email ?? new EmailSettings();
    }
}

public class EmailSettings
{
    public string Host { get; set; } = "smtp.yandex.ru";
    public int Port { get; set; } = 587;
    public string SenderEmail { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string Password { get; set; } = "";
}

public class Settings
{
    public string[] Prefixes { get; set; } = [];
    public string ConnectionString { get; set; } = string.Empty;
    public EmailSettings Email { get; set; } = new();
}