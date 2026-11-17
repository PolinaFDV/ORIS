using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Results;

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

        // Ищем контроллеры во ВСЕХ загруженных сборках
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
                       attribute.Route.Equals(
                           segments[0],
                           StringComparison.OrdinalIgnoreCase);
            });

        if (controllerType == null || !typeof(BaseController).IsAssignableFrom(controllerType))
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

            BaseController? controller =
                Activator.CreateInstance(controllerType) as BaseController;

            if (controller == null)
            {
                throw new InvalidOperationException(
                    "Контроллер должен наследоваться от BaseController.");
            }

            controller.SetContext(context);

            object? result = action.Invoke(controller, arguments);

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

            if (!context.Response.OutputStream.CanWrite)
            {
                return;
            }

            await new HtmlResult(
                "<h1>500 - Метод контроллера вернул недопустимый результат</h1>",
                500).ExecuteAsync(context.Response);
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
                await new HtmlResult(
                    "<h1>500 - Ошибка сервера</h1>",
                    500).ExecuteAsync(context.Response);
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
                if (query.TryGetValue(name, out string? qValue))
                {
                    arguments[i] = ConvertValue(qValue, parameter);
                }
                else if (form.TryGetValue(name, out string? fValue))
                {
                    arguments[i] = ConvertValue(fValue, parameter);
                }
                else
                {
                    arguments[i] = parameter.HasDefaultValue
                        ? parameter.DefaultValue
                        : ConvertValue(null, parameter);
                }
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

        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            throw new ArgumentException(
                $"Некорректное значение параметра: {parameter.Name}");
        }
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