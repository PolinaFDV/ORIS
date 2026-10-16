

using MyHttpServer.Framework;
using MyHttpServer.Framework.Core;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;

namespace MyHttpServer.Framework.Controllers;

[HttpController("home-work")]
public class HomeworkController : ControllerBase
{
    [Get("send")]
    public string Send([FromQuery] string to)
    {
        // Проверяем адрес получателя
        if (!MailAddress.TryCreate(to, out _))
            throw new ArgumentException("Некорректный адрес почты.");

        // Данные отправителя и студента
        string senderEmail = "f3dyaeva.p@yandex.ru";
        string appPassword = "rczhdkvnceubjdhd";
        string group = "11-541";
        string fullName = "Федяева Полина Константиновна";

        // Находим папку проекта
        string projectDirectory = FindProjectDirectory();

        // Создаём временный ZIP-архив
        string zipPath = Path.Combine(
            Path.GetTempPath(),
            "MyHttpServer.zip"
        );

        try
        {
            using (FileStream zipFile = new FileStream(
                       zipPath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(
                       zipFile, ZipArchiveMode.Create))
            {
                string[] files = Directory.GetFiles(
                    projectDirectory, "*", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    string relativePath = Path.GetRelativePath(
                        projectDirectory, file);

                    // Не добавляем служебные папки
                    if (relativePath.Split(Path.DirectorySeparatorChar)
                        .Any(part => part == "bin" ||
                                     part == "obj" ||
                                     part == ".git" ||
                                     part == ".vs"))
                    {
                        continue;
                    }

                    archive.CreateEntryFromFile(file, relativePath);
                }
            }

            // Создаём письмо
            MailAddress from = new MailAddress(senderEmail, fullName);
            MailAddress toAddress = new MailAddress(to);

            using (MailMessage message = new MailMessage(from, toAddress))
            {
                message.Subject = "Домашнее задание 5";
                message.Body =
                    $"Номер группы: {group}\n" +
                    $"ФИО: {fullName}\n\n";

                // Прикрепляем архив
                message.Attachments.Add(new Attachment(zipPath));

                // Настраиваем SMTP Яндекса
                using (SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587))
                {
                    smtp.Credentials = new NetworkCredential(
                        senderEmail, appPassword);

                    smtp.EnableSsl = true;
                    smtp.Send(message);
                }
            }

            return "Письмо успешно отправлено!";
        }
        finally
        {
            // Удаляем временный архив после отправки
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

