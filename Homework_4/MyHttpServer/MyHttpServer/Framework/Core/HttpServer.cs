using System;
using System.Net;
using System.Threading.Tasks;
using MyHttpServer.Framework.Handlers;

namespace MyHttpServer.Framework.Core
{
    public class HttpServer
    {
        private HttpListener? _listener;
        private readonly BaseHandler _handlerChain;

        public HttpServer()
        {
            var staticHandler = new StaticFilesHandler();
            var notFoundHandler = new NotFoundHandler();

            staticHandler.SetNext(notFoundHandler);
            
            _handlerChain = staticHandler;
        }

        public void Start()
        {
            _listener = new HttpListener();

            foreach (var prefix in Settings.Instance.Prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }

            _listener.Start();
            Console.WriteLine("Сервер начал свою работу");
            
            Console.WriteLine("\nДоступные страницы:");
            Console.WriteLine("http://127.0.0.1:8888/index.html");
            Console.WriteLine("http://127.0.0.1:8888/login.html");
            Console.WriteLine("http://127.0.0.1:8888/register.html");
            Console.WriteLine("http://127.0.0.1:8888/Steam.html");
            Console.WriteLine("http://127.0.0.1:8888/404.html\n");

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

            Console.WriteLine($"[Запрос] {context.Request.HttpMethod} {context.Request.Url.AbsolutePath}");

            try
            {
                await _handlerChain.HandleRequestAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка в цепочке] {ex.Message}");
                try
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
                catch { }
            }
        }
    }
}