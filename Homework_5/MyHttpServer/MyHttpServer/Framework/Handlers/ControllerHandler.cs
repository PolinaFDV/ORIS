
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using MyHttpServer.Framework;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Http;
using MyHttpServer.Framework.HttpResponses;

namespace MyHttpServer.Framework.Handlers;

internal sealed class ControllerHandler : Handler
{
    public override async Task HandleRequestAsync(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;

        string[] segments = (request.Url?.AbsolutePath ?? "")
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length != 2)
        {
            await NextAsync(context);
            return;
        }

        Type? controllerType = Assembly.GetExecutingAssembly()
            .GetTypes()
            .FirstOrDefault(type =>
            {
                HttpControllerAttribute? attribute =
                    type.GetCustomAttribute<HttpControllerAttribute>();

                return attribute != null &&
                       attribute.Route.Equals(
                           segments[0],
                           StringComparison.OrdinalIgnoreCase);
            });

        if (controllerType == null)
        {
            await NextAsync(context);
            return;
        }

        MethodInfo? action = controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(method =>
                MatchesRoute(method, request.HttpMethod, segments[1]));

        if (action == null)
        {
            await NextAsync(context);
            return;
        }

        try
        {
            object?[] arguments = await BindParameters(action, request);

            ControllerBase? controller =
                Activator.CreateInstance(controllerType) as ControllerBase;

            if (controller == null)
            {
                throw new InvalidOperationException(
                    "Контроллер должен наследоваться от ControllerBase.");
            }

            controller.SetContext(context);

            object? result = action.Invoke(controller, arguments);

            if (result is Task task)
            {
                await task;
                return;
            }

            if (result is IHttpResponseTypeResult responseResult)
            {
                byte[] bytes = responseResult.Execute(context);
                await HttpResponseWriter.WriteBytesAsync(context.Response, bytes);
            }
            else
            {
                string html = result as string ?? "<p>Метод выполнен.</p>";
                await HttpResponseWriter.WriteHtmlAsync(context.Response, html);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка контроллера: {ex.GetBaseException().Message}");

            if (!context.Response.OutputStream.CanWrite)
            {
                return;
            }

            try
            {
                await HttpResponseWriter.WriteHtmlAsync(
                    context.Response,
                    "<h1>500 - Ошибка сервера</h1>",
                    500);
            }
            catch
            {
                context.Response.Close();
            }
        }
    }

    private static bool MatchesRoute(
        MethodInfo method,
        string httpMethod,
        string route)
    {
        RouteAttribute? attribute = method.GetCustomAttribute<RouteAttribute>();

        return attribute != null
               && attribute.Method.Equals(
                   httpMethod,
                   StringComparison.OrdinalIgnoreCase)
               && attribute.Path.Trim('/').Equals(
                   route,
                   StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<object?[]> BindParameters(
        MethodInfo action,
        HttpListenerRequest request)
    {
        ParameterInfo[] parameters = action.GetParameters();
        object?[] arguments = new object?[parameters.Length];

        Dictionary<string, string> query =
            ParseUrlEncoded(request.Url?.Query.TrimStart('?') ?? "");

        Dictionary<string, string> form = new Dictionary<string, string>();

        if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new System.IO.StreamReader(
                request.InputStream,
                request.ContentEncoding);

            string body = await reader.ReadToEndAsync();
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
                arguments[i] = parameter.HasDefaultValue
                    ? parameter.DefaultValue
                    : ConvertValue(null, parameter);
            }
        }

        return arguments;
    }

    private static object? ConvertValue(string? value, ParameterInfo parameter)
    {
        if (value == null)
        {
            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            if (Nullable.GetUnderlyingType(parameter.ParameterType) != null
                || !parameter.ParameterType.IsValueType)
            {
                return null;
            }

            throw new ArgumentException(
                $"Не передан параметр: {parameter.Name}");
        }

        Type targetType =
            Nullable.GetUnderlyingType(parameter.ParameterType)
            ?? parameter.ParameterType;

        return Convert.ChangeType(value, targetType);
    }

    private static Dictionary<string, string> ParseUrlEncoded(string data)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        foreach (string part in data.Split(
                     '&',
                     StringSplitOptions.RemoveEmptyEntries))
        {
            string[] pair = part.Split('=', 2);

            string key = WebUtility.UrlDecode(pair[0]) ?? "";

            string value = pair.Length > 1
                ? WebUtility.UrlDecode(pair[1]) ?? ""
                : "";

            result[key] = value;
        }

        return result;
    }
}
