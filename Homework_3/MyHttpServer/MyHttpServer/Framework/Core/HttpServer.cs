using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Core
{
    public class HttpServer
    {
        private int _port = 8888;
        private string filePath = Path.Combine(AppContext.BaseDirectory, "static", "index.html");
        private HttpListener _listener;

        public void Start()
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Ошибка: файл {filePath} не найден!");
                Console.WriteLine("Сервер не может быть запущен.");
                Console.ReadLine();
                return;
            }

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://127.0.0.1:" + _port.ToString() + "/");
            _listener.Start();
            Console.WriteLine("Сервер начал свою работу");

            Receive();
        }

        public void Stop()
        {
            _listener.Stop();
            Console.WriteLine("Сервер завершил свою работу");
        }

        private void Receive()
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        private async void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {
                var context = _listener.EndGetContext(result);
                var request = context.Request;
                Console.WriteLine("Пришел запрос");


                string path = request.Url.LocalPath;
                if (path == "/")
                {
                    path = "/index.html";
                }
                
                var response = context.Response;

                string relativePath = path.TrimStart('/');
                string filePath = Path.Combine(AppContext.BaseDirectory, "static", relativePath);

                if (!File.Exists(filePath))
                {
                    response.StatusCode = 404;
                    filePath = Path.Combine(AppContext.BaseDirectory, "static", "404.html");
                }

                response.ContentType = ContentTypeProvider.GetContentType(filePath);
                
                byte[] buffer = await File.ReadAllBytesAsync(filePath);
                response.ContentLength64 = buffer.Length;
                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer);
                await output.FlushAsync();

                Console.WriteLine("Запрос обработан");
                Receive();

            }
        }
    }
}