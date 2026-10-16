using MyHttpServer.Framework.Core;
using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Net.Mail;

namespace MyHttpServer.Framework.Controllers;
[HttpController("steam")]
public class SteamController : ControllerBase
{
    [Get("send")]
    public void Send([FromQuery] string to, string hero)
    {
        try
        {
            // отправитель - устанавливаем адрес и отображаемое в письме имя
            MailAddress from = new MailAddress("f3dyaeva.p@yandex.ru", "PolinaF");
            // кому отправляем
            MailAddress toAddress = new MailAddress(to);
            // создаем объект сообщения
            MailMessage message = new MailMessage(from, toAddress);
            // тема письма
            message.Subject = "Тест";
            // текст письма
            message.Body = "<h2>Письмо-тест работы smtp-клиента</h2>";
            // письмо представляет код html
            message.IsBodyHtml = true;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587);
            // логин и пароль
            smtp.Credentials = new NetworkCredential("f3dyaeva.p@yandex.ru", "rczhdkvnceubjdhd");
            smtp.EnableSsl = true;
            smtp.Send(message);
            Console.WriteLine($"Письмо отправлено на {to}");
            Console.Read();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MAIL ERROR] {ex.Message}");
        }
    }
}