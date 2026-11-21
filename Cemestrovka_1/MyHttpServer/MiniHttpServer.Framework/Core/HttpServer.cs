using System.Net;
using System.Text.Json;
using MyHttpServer.Framework.Authentication;
using MyHttpServer.Framework.Handlers;

namespace MyHttpServer.Framework.Core;

public class HttpServer
{
    private HttpListener? _listener;
    private readonly Settings _settings;

    private readonly StaticFileHandler _staticFileHandler = new();
    private readonly ControllerHandler _controllerHandler;
    private readonly NotFoundHandler _notFoundHandler = new();

    public HttpServer()
    {
        string settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        _settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsPath))
                    ?? throw new InvalidOperationException("Не удалось прочитать settings.json");

        IUserSessionService sessionService = new UserSessionService();
        _controllerHandler = new ControllerHandler(sessionService);

        _staticFileHandler.SetNext(_controllerHandler);
        _controllerHandler.SetNext(_notFoundHandler);
    }

    public void Start()
    {
        _listener = new HttpListener();

        foreach (string prefix in _settings.Prefixes)
        {
            _listener.Prefixes.Add(prefix);
        }

        _listener.Start();

        foreach (string prefix in _settings.Prefixes)
        {
            Console.WriteLine($"Сервер запущен: {prefix}");
        }

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
            _listener.BeginGetContext(ListenerCallback, null);
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
            Console.WriteLine($"{context.Request.HttpMethod} {context.Request.Url?.AbsolutePath}");

            await _staticFileHandler.HandleRequestAsync(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сервера: {ex.GetBaseException().Message}");

            try
            {
                if (context.Response.OutputStream.CanWrite)
                {
                    context.Response.StatusCode = 500;
                }
            }
            catch { }

            try { context.Response.Close(); } catch { }
        }
    }
}