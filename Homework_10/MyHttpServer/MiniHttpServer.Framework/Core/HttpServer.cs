using System;
using System.Net;
using System.Threading.Tasks;
using MyHttpServer.Framework.Authentication;
using MyHttpServer.Framework.Handlers;
using MyHttpServer.Framework.Authentication;

namespace MyHttpServer.Framework.Core;

public class HttpServer
{
    private readonly int _port = 8888;
    private HttpListener? _listener;

    private readonly StaticFileHandler _staticFileHandler = new StaticFileHandler();
    private readonly ControllerHandler _controllerHandler;
    private readonly NotFoundHandler _notFoundHandler = new NotFoundHandler();

    public HttpServer()
    {
        IUserSessionService sessionService = new UserSessionService();
        _controllerHandler = new ControllerHandler(sessionService);

        _staticFileHandler.SetNext(_controllerHandler);
        _controllerHandler.SetNext(_notFoundHandler);
    }

    public void Start()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
        _listener.Start();

        Console.WriteLine($"Сервер запущен: http://127.0.0.1:{_port}/");
        Receive();
    }

    public void Stop()
    {
        if (_listener != null && _listener.IsListening)
        {
            _listener.Stop();
            _listener.Close();
            Console.WriteLine("Сервер остановлен.");
        }
    }

    private void Receive()
    {
        if (_listener != null && _listener.IsListening)
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), null);
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

        Console.WriteLine($"{context.Request.HttpMethod} {context.Request.Url?.AbsolutePath}");

        try
        {
            await _staticFileHandler.HandleRequestAsync(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сервера: {ex.GetBaseException().Message}");

            try
            {
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
            catch { }
        }
    }
}