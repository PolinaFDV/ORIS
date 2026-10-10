using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using MyHttpServer.Handlers;

namespace MyHttpServer.Framework.Core
{
    public class HttpServer
    {
        private int _port = 8888;

        private string filePath = Path.Combine(
            AppContext.BaseDirectory,
            "static",
            "login.html"
        );

        private HttpListener _listener;

        private readonly object[] _handlers =
        {
            new LoginHandler()
        };

        public void Start()
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Ошибка: файл {filePath} не найден!");
                Console.WriteLine("Сервер не может быть запущен.");
                return;
            }

            _listener = new HttpListener();

            _listener.Prefixes.Add(
                "http://127.0.0.1:" + _port.ToString() + "/"
            );

            _listener.Start();

            Console.WriteLine("Сервер начал свою работу");

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
            if (_listener.IsListening)
            {
                _listener.BeginGetContext(
                    new AsyncCallback(ListenerCallback),
                    _listener
                );
            }
        }

        private async void ListenerCallback(IAsyncResult result)
        {
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

            string path = request.Url.AbsolutePath;

            Console.WriteLine(
                $"Запрос: {request.HttpMethod} {path}"
            );

            foreach (object handler in _handlers)
            {
                MethodInfo[] methods = handler.GetType().GetMethods();

                foreach (MethodInfo method in methods)
                {
                    RouteAttribute attribute =
                        method.GetCustomAttribute<RouteAttribute>();

                    if (attribute == null)
                    {
                        continue;
                    }

                    if (attribute.Method != request.HttpMethod ||
                        attribute.Path != path)
                    {
                        continue;
                    }

                    object result = method.Invoke(
                        handler,
                        new object[] { context }
                    );

                    Task task = result as Task;

                    if (task != null)
                    {
                        await task;
                    }

                    Console.WriteLine("Обработчик выполнен");
                    return;
                }
            }

            if (request.HttpMethod == "GET")
            {
                if (path == "/" || path == "/login")
                {
                    path = "/login.html";
                }

                string relativePath = path.TrimStart('/');

                string staticFolder = Path.Combine(
                    AppContext.BaseDirectory,
                    "static"
                );

                string requestedFile = Path.GetFullPath(
                    Path.Combine(staticFolder, relativePath)
                );

                string folderPrefix = Path.GetFullPath(staticFolder)
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

                    byte[] notFound = System.Text.Encoding.UTF8.GetBytes(
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
                    ContentTypeProvider.GetContentType(requestedFile);

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
    }
}