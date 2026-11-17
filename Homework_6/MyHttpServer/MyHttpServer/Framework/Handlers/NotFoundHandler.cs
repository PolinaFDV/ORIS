using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Handlers;

public class NotFoundHandler : Handler
{
    public override async Task HandleRequestAsync(HttpListenerContext context)
    {
        string notFoundPath = Path.Combine(
            AppContext.BaseDirectory, "static", "404.html");

        context.Response.StatusCode = 404;

        if (File.Exists(notFoundPath))
        {
            context.Response.ContentType = "text/html; charset=utf-8";

            byte[] bytes = await File.ReadAllBytesAsync(notFoundPath);
            context.Response.ContentLength64 = bytes.Length;
            await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }
        else
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            byte[] bytes = Encoding.UTF8.GetBytes("404 — Не найдено");
            context.Response.ContentLength64 = bytes.Length;
            await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }

        context.Response.Close();
    }
}