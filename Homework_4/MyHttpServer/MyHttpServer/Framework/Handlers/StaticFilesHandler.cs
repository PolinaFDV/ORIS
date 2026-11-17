using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using MyHttpServer.Framework.Core;

namespace MyHttpServer.Framework.Handlers
{
    public class StaticFilesHandler : BaseHandler
    {
        private readonly string _staticFolder = Path.Combine(AppContext.BaseDirectory, "static");

                public override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            string path = request.Url?.AbsolutePath ?? "/";

            if (request.HttpMethod != "GET")
            {
                await base.HandleRequestAsync(context);
                return;
            }

            if (path == "/")
            {
                path = "/SearcherFinder/index.html";
            }

            string relativePath = path.TrimStart('/');
            string requestedFile = Path.GetFullPath(Path.Combine(_staticFolder, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            string folderPrefix = Path.GetFullPath(_staticFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!requestedFile.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = 403;
                response.Close();
                return;
            }

            if (File.Exists(requestedFile))
            {
                response.ContentType = ContentTypeProvider.GetContentType(requestedFile);
                response.Headers.Add("Server", "MyHttpServer"); 

                byte[] buffer = await File.ReadAllBytesAsync(requestedFile);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
                
                Console.WriteLine($"[StaticFilesHandler] 200 OK: {relativePath}");
                return; 
            }

            Console.WriteLine($"[StaticFilesHandler] Файл не найден: {relativePath}. Передаем в NotFoundHandler...");
            await base.HandleRequestAsync(context);
        }
    }
}