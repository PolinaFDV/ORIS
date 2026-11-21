using System.IO;
using System.Net;
using System.Threading.Tasks;
using MyHttpServer.Framework.Http;

namespace MyHttpServer.Framework.Handlers;

public class NotFoundHandler : Handler
{
    public override async Task HandleRequestAsync(HttpListenerContext context)
    {
        string notFoundPath = Path.Combine(AppContext.BaseDirectory, "static", "404.html");

        if (File.Exists(notFoundPath))
        {
            byte[] bytes = await File.ReadAllBytesAsync(notFoundPath);
            await HttpResponseWriter.WriteBytesAsync(
                context.Response, bytes, "text/html; charset=utf-8", 404);
        }
        else
        {
            await HttpResponseWriter.WriteTextAsync(
                context.Response, "404 — Не найдено", "text/plain; charset=utf-8", 404);
        }
    }
}