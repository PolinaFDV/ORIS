using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Core
{
    public class HttpServer
    {
        private readonly string _staticFolder = Path.Combine(AppContext.BaseDirectory, "static");
        private HttpListener? _listener;

        public void Start()
        {
            _listener = new HttpListener();

            foreach (var prefix in ServerSettings.Instance.Prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }

            _listener.Start();
            Console.WriteLine("Сервер начал свою работу");
            
            foreach (var prefix in ServerSettings.Instance.Prefixes)
            {
                string baseUrl = prefix.TrimEnd('/');
                
                Console.WriteLine("\nДоступные страницы:");
                Console.WriteLine($"  Поисковик:   {baseUrl}/SearcherFinder/");
                Console.WriteLine($"  Satisfactory: {baseUrl}/Satisfactory/");
                Console.WriteLine($"  Steam:        {baseUrl}/Steam/");
                Console.WriteLine($"  Ошибка 404:   {baseUrl}/404.html");
                Console.WriteLine();
            }

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
                _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
            }
        }

        private async void ListenerCallback(IAsyncResult result)
        {
            if (_listener == null) return;

            HttpListenerContext context;
            try
            {
                context = _listener.EndGetContext(result);
            }
            catch (HttpListenerException) { return; }
            catch (ObjectDisposedException) { return; }

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
                catch { }
            }
        }

        private async Task ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string path = request.Url?.AbsolutePath ?? "/";
            Console.WriteLine($"Запрос: {request.HttpMethod} {path}");

            if (request.HttpMethod != "GET")
            {
                response.StatusCode = 405; 
                response.Close();
                return;
            }

            string relativePath = path.TrimStart('/');

            if (string.IsNullOrEmpty(relativePath) || path == "/")
            {
                relativePath = "SearcherFinder/index.html";
            }
            else if (path.EndsWith("/"))
            {
                string folderPath = Path.Combine(_staticFolder, relativePath.Replace('/', Path.DirectorySeparatorChar));
                string indexFile = Path.Combine(folderPath, "index.html");

                if (File.Exists(indexFile))
                {
                    relativePath += "index.html";
                }
                else
                {
                    string folderName = relativePath.TrimEnd('/');
                    string fallbackFile = Path.Combine(_staticFolder, folderName + ".html");
                    
                    if (File.Exists(fallbackFile))
                    {
                        relativePath = folderName + ".html";
                    }
                    else
                    {
                        relativePath += "index.html"; 
                    }
                }
            }

            string requestedFile = Path.GetFullPath(Path.Combine(_staticFolder, relativePath.Replace('/', Path.DirectorySeparatorChar)));

            string folderPrefix = Path.GetFullPath(_staticFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!requestedFile.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = 403; 
                response.Close();
                return;
            }

            if (!File.Exists(requestedFile))
            {
                response.StatusCode = 404;
                response.ContentType = "text/html; charset=utf-8";
                byte[] notFound = Encoding.UTF8.GetBytes("<h1>404 - Файл не найден</h1>");
                response.ContentLength64 = notFound.Length;
                await response.OutputStream.WriteAsync(notFound, 0, notFound.Length);
                response.Close();
                Console.WriteLine("Возвращен 404");
                return;
            }

            response.ContentType = GetContentType(requestedFile);
            response.Headers.Add("Server", "MyHttpServer"); 

            byte[] buffer = await File.ReadAllBytesAsync(requestedFile);
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
            Console.WriteLine($"Файл отправлен: {relativePath}");
        }

        private static string GetContentType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();

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
                ".webp" => "image/webp",
                _ => "application/octet-stream", 
            };
        }
    }
}