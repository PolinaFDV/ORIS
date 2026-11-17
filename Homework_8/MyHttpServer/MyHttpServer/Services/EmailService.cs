using System;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;
using MyHttpServer.Framework.Core;

namespace MyHttpServer.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService()
    {
        _settings = LoadSettings();
    }

    public async Task<bool> SendAsync(string to, string subject, string body)
    {
        try
        {
            MailAddress from = new MailAddress(_settings.SenderEmail, _settings.SenderName);
            MailAddress toAddress = new MailAddress(to);

            using MailMessage message = new MailMessage(from, toAddress);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using SmtpClient smtp = new SmtpClient(_settings.Host, _settings.Port);
            smtp.Credentials = new NetworkCredential(
                _settings.SenderEmail, _settings.Password);
            smtp.EnableSsl = true;

            await smtp.SendMailAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MAIL ERROR] {ex.Message}");
            return false;
        }
    }

    private static EmailSettings LoadSettings()
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