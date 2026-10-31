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
        private HttpListener _listener;
        private readonly Settings _settings;

        private readonly object[] _handlers =
        {
            new LoginHandler(),
            new ProductHandler() 
        };

        public HttpServer(Settings settings)
        {
            _settings = settings;
            _listener = new HttpListener();
            foreach (var prefix in _settings.Prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine($"Сервер запущен: http://127.0.0.1:8888/");
            Receive();
        }

        public void Stop()
        {
            if (_listener != null && _listener.IsListening)
            {
                _listener.Stop();
            }
        }

        private void Receive()
        {
            if (_listener.IsListening)
            {
                _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
            }
        }

        private async void ListenerCallback(IAsyncResult result)
        {
            HttpListenerContext context;
            try { context = _listener.EndGetContext(result); }
            catch { return; }

            Receive();
            try { await ProcessRequest(context); }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
        }

        private async Task ProcessRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;
            string method = context.Request.HttpMethod;

            Console.WriteLine($"Запрос: {method} {path}");

            foreach (object handler in _handlers)
            {
                foreach (MethodInfo m in handler.GetType().GetMethods())
                {
                    RouteAttribute attr = m.GetCustomAttribute<RouteAttribute>();
                    if (attr == null) continue;

                    if (attr.Method == method && attr.Path == path)
                    {
                        Console.WriteLine($"Найден маршрут: {path}");
                        object res = m.Invoke(handler, new object[] { context });
                        if (res is Task task) await task;
                        return; 
                    }
                }
            }

            if (method == "GET")
            {
                if (path == "/") path = "/index.html";
                
                string file = Path.Combine(AppContext.BaseDirectory, "static", path.TrimStart('/'));
                
                if (File.Exists(file))
                {
                    context.Response.ContentType = ContentTypeProvider.GetContentType(file);
                    byte[] buffer = await File.ReadAllBytesAsync(file);
                    context.Response.ContentLength64 = buffer.Length;
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    context.Response.Close();
                    return;
                }
                else
                {
                    context.Response.StatusCode = 404;
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    byte[] msg = System.Text.Encoding.UTF8.GetBytes("404 - Файл не найден: " + path);
                    context.Response.ContentLength64 = msg.Length;
                    await context.Response.OutputStream.WriteAsync(msg, 0, msg.Length);
                    context.Response.Close();
                    return;
                }
            }

            Console.WriteLine($"Маршрут НЕ найден для {method} {path}");
            context.Response.StatusCode = 404;
            context.Response.Close();
        }
    }
}