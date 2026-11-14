using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Core
{
    public class HttpServer
    {
        private int _port = 8888;

        private readonly string _rootFolder = Directory.GetCurrentDirectory();

        private HttpListener? _listener;

        public void Start()
        {
            string indexFile = Path.Combine(_rootFolder, "index.html");

            if (!File.Exists(indexFile))
            {
                Console.WriteLine($"Ошибка: файл {indexFile} не найден!");
                Console.WriteLine("Сервер не может быть запущен.");
                return;
            }

            _listener = new HttpListener();

            _listener.Prefixes.Add(
                "http://127.0.0.1:" + _port.ToString() + "/"
            );

            _listener.Start();

            Console.WriteLine("Сервер начал свою работу");
            Console.WriteLine($"Откройте в браузере: http://127.0.0.1:{_port}/connection/");

            Receive();
        }

        public void Stop()
        {
            if (_listener != null && _listener.IsListening)
            {
                _listener.Stop();
                Console.WriteLine("Сервер завершил свою работу");
            }
        }

        private void Receive()
        {
            if (_listener != null && _listener.IsListening)
            {
                _listener.BeginGetContext(
                    new AsyncCallback(ListenerCallback),
                    _listener
                );
            }
        }

        private async void ListenerCallback(IAsyncResult result)
        {
            if (_listener == null)
            {
                return;
            }

            HttpListenerContext context;

            try
            {
                context = _listener.EndGetContext(result);
            }
            catch (HttpListenerException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            Receive();

            try
            {
                await ProcessRequest(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");

                try
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
                catch
                {
                }
            }
        }

        private async Task ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;

            string path = request.Url?.AbsolutePath ?? "/";

            Console.WriteLine(
                $"Запрос: {request.HttpMethod} {path}"
            );

            if (request.HttpMethod == "GET")
            {
                if (path == "/" || path == "/connection" || path == "/connection/")
                {
                    path = "/index.html";
                }

                string relativePath = path.TrimStart('/');

                string requestedFile = Path.GetFullPath(
                    Path.Combine(_rootFolder, relativePath)
                );

                string folderPrefix = Path.GetFullPath(_rootFolder)
                    .TrimEnd(Path.DirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;

                if (!requestedFile.StartsWith(
                    folderPrefix,
                    StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = 403;
                    context.Response.Close();
                    return;
                }

                if (!File.Exists(requestedFile))
                {
                    context.Response.StatusCode = 404;
                    context.Response.ContentType =
                        "text/plain; charset=utf-8";

                    byte[] notFound = Encoding.UTF8.GetBytes(
                        "404 - Файл не найден"
                    );

                    context.Response.ContentLength64 = notFound.Length;

                    await context.Response.OutputStream.WriteAsync(
                        notFound, 0, notFound.Length
                    );

                    context.Response.Close();
                    return;
                }

                context.Response.ContentType =
                    GetContentType(requestedFile);

                byte[] buffer = await File.ReadAllBytesAsync(requestedFile);

                context.Response.ContentLength64 = buffer.Length;

                await context.Response.OutputStream.WriteAsync(
                    buffer, 0, buffer.Length
                );

                context.Response.Close();
                Console.WriteLine("Файл отправлен браузеру");
                return;
            }

            context.Response.StatusCode = 404;
            context.Response.Close();
        }

        private static string GetContentType(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();

            return ext switch
            {
                ".html" or ".htm" => "text/html; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".js" => "application/javascript; charset=utf-8",
                ".json" => "application/json; charset=utf-8",
                ".svg" => "image/svg+xml",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".ico" => "image/x-icon",
                ".woff" => "font/woff",
                ".woff2" => "font/woff2",
                ".ttf" => "font/ttf",
                _ => "application/octet-stream"
            };
        }
    }
}