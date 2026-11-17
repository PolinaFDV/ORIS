using MyHttpServer.Framework;
using MyHttpServer.Framework.Core;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using MyHttpServer.Framework.Results;

namespace MyHttpServer.Framework.Controllers;

[HttpController("home-work")]
public class HomeworkController : BaseController
{
    [Get("send")]
public IHttpResult Send([FromQuery] string to)
{
    if (!MailAddress.TryCreate(to, out _))
    {
        return Json(new { Success = false, Message = "Некорректный адрес почты" }, 400);
    }

    string senderEmail = "@yandex.ru";
    string appPassword = "";
    string group = "11-541";
    string fullName = "Федяева Полина Константиновна";

    string projectDirectory = FindProjectDirectory();

    string zipPath = Path.Combine(Path.GetTempPath(), "MyHttpServer.zip");

    try
    {
        using (FileStream zipFile = new FileStream(zipPath, FileMode.Create))
        using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Create))
        {
            string[] files = Directory.GetFiles(projectDirectory, "*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string relativePath = Path.GetRelativePath(projectDirectory, file);

                if (relativePath.Split(Path.DirectorySeparatorChar)
                    .Any(part => part == "bin" || part == "obj" || part == ".git" || part == ".vs"))
                {
                    continue;
                }

                archive.CreateEntryFromFile(file, relativePath);
            }
        }

        MailAddress from = new MailAddress(senderEmail, fullName);
        MailAddress toAddress = new MailAddress(to);

        using (MailMessage message = new MailMessage(from, toAddress))
        {
            message.Subject = "Домашнее задание 6";
            message.Body = $"{fullName}\nНомер группы: {group}\n\n";
            message.Attachments.Add(new Attachment(zipPath));

            using (SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587))
            {
                smtp.Credentials = new NetworkCredential(senderEmail, appPassword);
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
        }

        return Json(new { Success = true, Message = "Письмо успешно отправлено" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[HomeworkController ERROR] {ex.GetBaseException().Message}");
        return Json(new { Success = false, Message = "Не удалось отправить письмо" }, 500);
    }
    finally
    {
        if (File.Exists(zipPath))
            File.Delete(zipPath);
    }
}

    private static string FindProjectDirectory()
    {
        DirectoryInfo directory =
            new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (directory.GetFiles("*.csproj").Length > 0)
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Не удалось найти папку проекта с файлом .csproj.");
    }
}

