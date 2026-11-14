using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Core;
using System.Reflection;

namespace MyHttpServer.UnitTests;

[TestClass]
public class ControllerHandlerTests
{
    // Получаем тип ControllerHandler, который находится в другом пространстве имён.
    private static Type GetHandlerType()
    {
        Type? handlerType = typeof(HttpServer).Assembly.GetType(
            "MyHttpServer.Framework.Handlers.ControllerHandler");

        Assert.IsNotNull(handlerType);

        return handlerType;
    }

    // Получаем закрытый статический метод по его имени.
    private static MethodInfo GetPrivateMethod(string methodName)
    {
        MethodInfo? method = GetHandlerType().GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.IsNotNull(method);

        return method;
    }

    // Проверяем, что маршрут совпадает с указанным HTTP-методом.
    [TestMethod]
    public void MatchesRoute_CorrectMethodAndPath_ReturnsTrue()
    {
        // Arrange
        MethodInfo action = typeof(RouteTestController)
            .GetMethod(nameof(RouteTestController.Login))!;

        MethodInfo matchesRoute = GetPrivateMethod("MatchesRoute");

        // Act
        bool result = (bool)matchesRoute.Invoke(
            null,
            new object[] { action, "GET", "login" })!;

        // Assert
        Assert.IsTrue(result);
    }

    // Проверяем, что маршрут учитывает регистр букв без различий.
    [TestMethod]
    public void MatchesRoute_DifferentLetterCase_ReturnsTrue()
    {
        // Arrange
        MethodInfo action = typeof(RouteTestController)
            .GetMethod(nameof(RouteTestController.Login))!;

        MethodInfo matchesRoute = GetPrivateMethod("MatchesRoute");

        // Act
        bool result = (bool)matchesRoute.Invoke(
            null,
            new object[] { action, "get", "LOGIN" })!;

        // Assert
        Assert.IsTrue(result);
    }

    // Проверяем, что неправильный HTTP-метод не подходит к маршруту.
    [TestMethod]
    public void MatchesRoute_WrongMethod_ReturnsFalse()
    {
        // Arrange
        MethodInfo action = typeof(RouteTestController)
            .GetMethod(nameof(RouteTestController.Login))!;

        MethodInfo matchesRoute = GetPrivateMethod("MatchesRoute");

        // Act
        bool result = (bool)matchesRoute.Invoke(
            null,
            new object[] { action, "POST", "login" })!;

        // Assert
        Assert.IsFalse(result);
    }

    // Проверяем преобразование строки в целое число.
    [TestMethod]
    public void ConvertValue_StringToInt_ReturnsNumber()
    {
        // Arrange
        ParameterInfo parameter = typeof(ConvertTestController)
            .GetMethod(nameof(ConvertTestController.Number))!
            .GetParameters()[0];

        MethodInfo convertValue = GetPrivateMethod("ConvertValue");

        // Act
        object? result = convertValue.Invoke(
            null,
            new object?[] { "25", parameter });

        // Assert
        Assert.AreEqual(25, result);
    }

    // Проверяем, что отсутствующее значение необязательного параметра
    // заменяется значением по умолчанию.
    [TestMethod]
    public void ConvertValue_MissingOptionalValue_ReturnsDefaultValue()
    {
        // Arrange
        ParameterInfo parameter = typeof(ConvertTestController)
            .GetMethod(nameof(ConvertTestController.OptionalNumber))!
            .GetParameters()[0];

        MethodInfo convertValue = GetPrivateMethod("ConvertValue");

        // Act
        object? result = convertValue.Invoke(
            null,
            new object?[] { null, parameter });

        // Assert
        Assert.AreEqual(10, result);
    }

    // Проверяем, что отсутствие обязательного числового параметра вызывает ошибку.
    [TestMethod]
    public void ConvertValue_MissingRequiredValue_ThrowsException()
    {
        // Arrange
        ParameterInfo parameter = typeof(ConvertTestController)
            .GetMethod(nameof(ConvertTestController.Number))!
            .GetParameters()[0];

        MethodInfo convertValue = GetPrivateMethod("ConvertValue");

        bool exceptionWasThrown = false;

        // Act
        try
        {
            convertValue.Invoke(
                null,
                new object?[] { null, parameter });
        }
        catch (TargetInvocationException exception)
        {
            exceptionWasThrown =
                exception.InnerException is ArgumentException;
        }

        // Assert
        Assert.IsTrue(exceptionWasThrown);
    }

    // Проверяем, что строка параметров разбирается правильно.
    [TestMethod]
    public void ParseUrlEncoded_SeveralParameters_ReturnsDictionary()
    {
        // Arrange
        MethodInfo parseMethod = GetPrivateMethod("ParseUrlEncoded");

        string data = "login=player&age=20";

        // Act
        var result = (Dictionary<string, string>)parseMethod.Invoke(
            null,
            new object[] { data })!;

        // Assert
        Assert.AreEqual("player", result["login"]);
        Assert.AreEqual("20", result["age"]);
    }

    // Проверяем, что закодированные символы правильно декодируются.
    [TestMethod]
    public void ParseUrlEncoded_EncodedValue_ReturnsDecodedValue()
    {
        // Arrange
        MethodInfo parseMethod = GetPrivateMethod("ParseUrlEncoded");

        string data = "name=%D0%92%D0%B0%D0%BD%D1%8F";

        // Act
        var result = (Dictionary<string, string>)parseMethod.Invoke(
            null,
            new object[] { data })!;

        // Assert
        Assert.AreEqual("Ваня", result["name"]);
    }

    // Проверяем, что параметр без знака равенства получает пустое значение.
    [TestMethod]
    public void ParseUrlEncoded_ParameterWithoutValue_ReturnsEmptyString()
    {
        // Arrange
        MethodInfo parseMethod = GetPrivateMethod("ParseUrlEncoded");

        string data = "login";

        // Act
        var result = (Dictionary<string, string>)parseMethod.Invoke(
            null,
            new object[] { data })!;

        // Assert
        Assert.AreEqual("", result["login"]);
    }
    
    [TestMethod]
    public async Task BindParameters_GetRequest_ReturnsQueryValues()
    {
        // Arrange
        MethodInfo action = typeof(BindTestController)
            .GetMethod(nameof(BindTestController.QueryParameters))!;

        // Act
        object?[] result = await BindParametersAsync(
            action,
            "GET",
            "/?name=Polina&age=20");

        // Assert
        Assert.AreEqual("Polina", result[0]);
        Assert.AreEqual(20, result[1]);
    }

    [TestMethod]
    public async Task BindParameters_PostRequest_ReturnsFormValues()
    {
        // Arrange
        MethodInfo action = typeof(BindTestController)
            .GetMethod(nameof(BindTestController.FormParameters))!;

        // Act
        object?[] result = await BindParametersAsync(
            action,
            "POST",
            "/",
            "login=Polina&age=20");

        // Assert
        Assert.AreEqual("Polina", result[0]);
        Assert.AreEqual(20, result[1]);
    }

    [TestMethod]
    public async Task BindParameters_MissingOptionalValue_ReturnsDefaultValue()
    {
        // Arrange
        MethodInfo action = typeof(BindTestController)
            .GetMethod(nameof(BindTestController.OptionalParameter))!;

        // Act
        object?[] result = await BindParametersAsync(
            action,
            "GET",
            "/");

        // Assert
        Assert.AreEqual(10, result[0]);
    }
    
    private static async Task<object?[]> BindParametersAsync(
        MethodInfo action,
        string httpMethod,
        string path,
        string? body = null)
    {
        // Создаём временный TCP-порт.
        TcpListener portListener = new TcpListener(IPAddress.Loopback, 0);
        portListener.Start();

        int port = ((IPEndPoint)portListener.LocalEndpoint).Port;

        portListener.Stop();

        // Создаём временный HTTP-сервер.
        using HttpListener listener = new HttpListener();

        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();

        // Создаём HTTP-клиент.
        using HttpClient client = new HttpClient();

        // Отправляем запрос.
        Task<HttpResponseMessage> clientTask;

        if (httpMethod == "POST")
        {
            StringContent content = new StringContent(
                body ?? "",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            clientTask = client.PostAsync(
                $"http://127.0.0.1:{port}{path}",
                content);
        }
        else
        {
            clientTask = client.GetAsync(
                $"http://127.0.0.1:{port}{path}");
        }

        // Получаем запрос на временном сервере.
        HttpListenerContext context = await listener.GetContextAsync();

        // Находим приватный метод BindParameters.
        MethodInfo bindMethod = GetPrivateMethod("BindParameters");

        // Вызываем метод и получаем его результат.
        Task<object?[]> bindTask = (Task<object?[]>)bindMethod.Invoke(
            null,
            new object?[] { action, context.Request })!;

        object?[] result = await bindTask;

        // Завершаем HTTP-запрос.
        context.Response.StatusCode = 200;
        context.Response.Close();

        await clientTask;

        return result;
    }
}

// Вспомогательный контроллер для проверки маршрутов.
public class RouteTestController
{
    [Get("login")]
    public void Login()
    {
    }
}

// Вспомогательный контроллер для проверки преобразования параметров.
public class ConvertTestController
{
    public void Number(int number)
    {
    }

    public void OptionalNumber(int number = 10)
    {
    }
}

public class BindTestController
{
    public void QueryParameters(
        [FromQuery] string name,
        [FromQuery] int age)
    {
    }

    public void FormParameters(
        [FromForm] string login,
        [FromForm] int age)
    {
    }

    public void OptionalParameter(
        [FromQuery] int age = 10)
    {
    }
}
