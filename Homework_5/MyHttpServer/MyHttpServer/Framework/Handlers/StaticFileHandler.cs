
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MyHttpServer.Framework.Core;

namespace MyHttpServer.Framework.Handlers;

public class StaticFileHandler : Handler
{
    public override async Task HandleRequestAsync(HttpListenerContext context)
    {
        if (context.Request.HttpMethod != "GET")
        {
            await NextAsync(context);
            return;
        }

        string path = Uri.UnescapeDataString(
            context.Request.Url?.AbsolutePath ?? "/");

        if (path == "/" || path == "/login")
        {
            path = "/login.html";
        }

        string staticFolder = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "static"));

        string requestedFile = Path.GetFullPath(
            Path.Combine(staticFolder, path.TrimStart('/', '\\')));

        string folderPrefix = staticFolder.TrimEnd(
            Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!requestedFile.StartsWith(
                folderPrefix, StringComparison.OrdinalIgnoreCase)
            && !requestedFile.Equals(
                staticFolder, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 403;
            context.Response.Close();
            return;
        }

        if (!File.Exists(requestedFile))
        {
            string notFoundPath = Path.Combine(staticFolder, "404.html");

            context.Response.StatusCode = 404;

            if (File.Exists(notFoundPath))
            {
                context.Response.ContentType =
                    ContentTypeProvider.GetContentType(notFoundPath);

                byte[] notFoundBytes =
                    await File.ReadAllBytesAsync(notFoundPath);

                context.Response.ContentLength64 = notFoundBytes.Length;

                await context.Response.OutputStream.WriteAsync(
                    notFoundBytes, 0, notFoundBytes.Length);
            }
            else
            {
                context.Response.ContentType = "text/plain; charset=utf-8";

                byte[] errorBytes = Encoding.UTF8.GetBytes(
                    "404 — Файл не найден");

                context.Response.ContentLength64 = errorBytes.Length;

                await context.Response.OutputStream.WriteAsync(
                    errorBytes, 0, errorBytes.Length);
            }

            Console.WriteLine($"Файл не найден: {path}");
            context.Response.Close();
            return;
        }

        context.Response.ContentType =
            ContentTypeProvider.GetContentType(requestedFile);

        byte[] bytes = await File.ReadAllBytesAsync(requestedFile);

        context.Response.ContentLength64 = bytes.Length;

        await context.Response.OutputStream.WriteAsync(
            bytes, 0, bytes.Length);

        context.Response.Close();
    }
}
