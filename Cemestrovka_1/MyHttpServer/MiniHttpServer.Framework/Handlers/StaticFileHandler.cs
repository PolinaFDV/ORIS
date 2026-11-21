using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Http;

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

        string path;
        try
        {
            path = Uri.UnescapeDataString(context.Request.Url?.AbsolutePath ?? "/");
        }
        catch
        {
            await HttpResponseWriter.WriteTextAsync(
                context.Response, "400 — Некорректный запрос", "text/plain; charset=utf-8", 400);
            return;
        }

        if (path == "/" || path == "/index" || path == "/index.html")
        {
            await NextAsync(context);
            return;
        }

        string staticFolder = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "static"));

        string requestedFile = Path.GetFullPath(
            Path.Combine(staticFolder, path.TrimStart('/', '\\')));

        string folderPrefix = staticFolder.TrimEnd(
            Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!requestedFile.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase)
            && !requestedFile.Equals(staticFolder, StringComparison.OrdinalIgnoreCase))
        {
            await HttpResponseWriter.WriteTextAsync(
                context.Response, "403 — Доступ запрещён", "text/plain; charset=utf-8", 403);
            return;
        }

        if (!File.Exists(requestedFile))
        {
            await NextAsync(context);
            return;
        }

        byte[] bytes = await File.ReadAllBytesAsync(requestedFile);
        await HttpResponseWriter.WriteBytesAsync(
            context.Response, bytes, ContentTypeProvider.GetContentType(requestedFile));
    }
}