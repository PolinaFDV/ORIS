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
            await NextAsync(context);
            return;
        }

        context.Response.ContentType =
            ContentTypeProvider.GetContentType(requestedFile);

        byte[] bytes = await File.ReadAllBytesAsync(requestedFile);
        context.Response.ContentLength64 = bytes.Length;
        await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        context.Response.Close();
    }
}