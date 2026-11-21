using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Authentication;
using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Results;
using MyHttpServer.Framework.Http;

namespace MyHttpServer.Framework.Handlers;

public sealed class ControllerHandler : Handler
{
    private const long MaxBodySize = 1024 * 1024; 

    private readonly IUserSessionService _sessionService;

    private static readonly ConcurrentDictionary<string, (Type Controller, MethodInfo Action)> RouteCache = new();

    public ControllerHandler(IUserSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public override async Task HandleRequestAsync(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;

        string[] segments = (request.Url?.AbsolutePath ?? "")
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            segments = new[] { "home", "index" };
        }

        if (segments.Length != 2)
        {
            await NextAsync(context);
            return;
        }

        string cacheKey = $"{segments[0].ToLowerInvariant()}/{request.HttpMethod}/{segments[1].ToLowerInvariant()}";

        if (!RouteCache.TryGetValue(cacheKey, out var entry))
        {
            entry = ResolveRoute(segments[0], request.HttpMethod, segments[1]);
            if (entry.Controller == null || entry.Action == null)
            {
                RouteCache[cacheKey] = (null!, null!);
                await NextAsync(context);
                return;
            }
            RouteCache[cacheKey] = entry;
        }

        if (entry.Controller == null || entry.Action == null)
        {
            await NextAsync(context);
            return;
        }

        if (!IsAuthorized(context, entry.Controller, entry.Action))
        {
            await HttpResponseWriter.WriteTextAsync(
                context.Response, "401 — Требуется авторизация", "text/plain; charset=utf-8", 401);
            return;
        }

        try
        {
            object?[] arguments = await BindParameters(entry.Action, request);

            BaseController? controller =
                Activator.CreateInstance(entry.Controller) as BaseController;

            if (controller == null)
            {
                throw new InvalidOperationException(
                    "Контроллер должен наследоваться от BaseController.");
            }

            controller.Initialize(context, _sessionService);

            object? result = entry.Action.Invoke(controller, arguments);

            if (result is Task task)
            {
                await task;

                PropertyInfo? resultProperty = task.GetType().GetProperty("Result");
                result = resultProperty?.GetValue(task);
            }

            if (result is IHttpResult httpResult)
            {
                await httpResult.ExecuteAsync(context.Response);
                return;
            }

            if (!context.Response.OutputStream.CanWrite) return;

            await new HtmlResult(
                "<h1>500 - Метод контроллера вернул недопустимый результат</h1>",
                500).ExecuteAsync(context.Response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка контроллера: {ex.GetBaseException().Message}");

            if (!context.Response.OutputStream.CanWrite) return;

            try
            {
                string errorPath = Path.Combine(AppContext.BaseDirectory, "static", "500.html");
                string html = File.Exists(errorPath) ? File.ReadAllText(errorPath) : "<h1>500 - Ошибка сервера</h1>";
                await new HtmlResult(html, 500).ExecuteAsync(context.Response);
            }
            catch
            {
                try { context.Response.Close(); } catch { }
            }
        }
    }

    private static (Type Controller, MethodInfo Action) ResolveRoute(
        string route, string httpMethod, string actionPath)
    {
        Type? controllerType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly =>
            {
                try { return assembly.GetTypes(); }
                catch { return Array.Empty<Type>(); }
            })
            .FirstOrDefault(type =>
            {
                HttpControllerAttribute? attribute =
                    type.GetCustomAttribute<HttpControllerAttribute>();
                return attribute != null &&
                       attribute.Route.Equals(route, StringComparison.OrdinalIgnoreCase);
            });

        if (controllerType == null || !typeof(BaseController).IsAssignableFrom(controllerType))
            return (null!, null!);

        MethodInfo? action = controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(method => MatchesRoute(method, httpMethod, actionPath));

        return (controllerType, action!);
    }

    internal bool IsAuthorized(HttpListenerContext context, Type controllerType, MethodInfo method)
    {
        bool requiresAuthorization =
            controllerType.IsDefined(typeof(AuthorizeAttribute), true)
            || method.IsDefined(typeof(AuthorizeAttribute), true);

        if (!requiresAuthorization) return true;

        string? sessionId = context.Request.Cookies["sessionId"]?.Value;
        int? userId = _sessionService.GetUserId(sessionId);

        return userId.HasValue;
    }

    internal static bool MatchesRoute(MethodInfo method, string httpMethod, string route)
    {
        RouteAttribute? attribute = method.GetCustomAttribute<RouteAttribute>();
        return attribute != null
               && attribute.Method.Equals(httpMethod, StringComparison.OrdinalIgnoreCase)
               && attribute.Path.Trim('/').Equals(route, StringComparison.OrdinalIgnoreCase);
    }

    internal static async Task<object?[]> BindParameters(MethodInfo action, HttpListenerRequest request)
    {
        if (request.ContentLength64 > MaxBodySize)
        {
            throw new ArgumentException($"Тело запроса превышает {MaxBodySize} байт.");
        }

        ParameterInfo[] parameters = action.GetParameters();
        object?[] arguments = new object?[parameters.Length];

        Dictionary<string, string> query =
            ParseUrlEncoded(request.Url?.Query.TrimStart('?') ?? "");

        Dictionary<string, string> form = new();

        if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding);

            char[] buffer = new char[MaxBodySize];
            int read = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
            string body = new string(buffer, 0, read);

            form = ParseUrlEncoded(body);
        }

        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            string name = parameter.Name ?? "";

            if (parameter.GetCustomAttribute<FromQueryAttribute>() != null)
            {
                query.TryGetValue(name, out string? value);
                arguments[i] = ConvertValue(value, parameter);
            }
            else if (parameter.GetCustomAttribute<FromFormAttribute>() != null)
            {
                form.TryGetValue(name, out string? value);
                arguments[i] = ConvertValue(value, parameter);
            }
            else
            {
                if (query.TryGetValue(name, out string? qValue))
                    arguments[i] = ConvertValue(qValue, parameter);
                else if (form.TryGetValue(name, out string? fValue))
                    arguments[i] = ConvertValue(fValue, parameter);
                else
                    arguments[i] = parameter.HasDefaultValue
                        ? parameter.DefaultValue
                        : ConvertValue(null, parameter);
            }
        }

        return arguments;
    }

    internal static object? ConvertValue(string? value, ParameterInfo parameter)
    {
        if (value == null)
        {
            if (parameter.HasDefaultValue) return parameter.DefaultValue;

            if (Nullable.GetUnderlyingType(parameter.ParameterType) != null
                || !parameter.ParameterType.IsValueType)
                return null;

            throw new ArgumentException($"Не передан параметр: {parameter.Name}");
        }

        Type targetType = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;

        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            throw new ArgumentException($"Некорректное значение параметра: {parameter.Name}");
        }
    }

    internal static Dictionary<string, string> ParseUrlEncoded(string data)
    {
        Dictionary<string, string> result = new();

        foreach (string part in data.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] pair = part.Split('=', 2);
            string key = WebUtility.UrlDecode(pair[0]) ?? "";
            string value = pair.Length > 1 ? WebUtility.UrlDecode(pair[1]) ?? "" : "";
            result[key] = value;
        }

        return result;
    }
}