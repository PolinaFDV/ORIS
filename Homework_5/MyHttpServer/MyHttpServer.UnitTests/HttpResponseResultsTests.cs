using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Core;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using MyHttpServer.Framework;

namespace MyHttpServer.UnitTests;

[TestClass]
public class HttpResponseResultsTests
{
    // Проверяем, что HtmlResult возвращает HTML и правильный Content-Type.
    [TestMethod]
    public async Task HtmlResult_ReturnsHtmlAndCorrectContentType()
    {
        // Подготавливаем HTML.
        string html = "<h1>Привет!</h1>";

        // Создаём результат через тестовый контроллер.
        TestController controller = new TestController();
        object result = controller.CreateHtmlResult(html);

        // Выполняем результат и получаем ответ.
        (string contentType, string body) = await ExecuteResultAsync(result);

        // Проверяем тип содержимого.
        Assert.AreEqual("text/html; charset=utf-8", contentType);

        // Проверяем, что HTML вернулся без изменений.
        Assert.AreEqual(html, body);
    }

    // Проверяем, что JsonResult правильно преобразует объект в JSON.
    [TestMethod]
    public async Task JsonResult_ReturnsJsonAndCorrectContentType()
    {
        // Создаём объект для преобразования в JSON.
        object user = new { Name = "Ваня", Age = 20 };

        // Создаём результат через тестовый контроллер.
        TestController controller = new TestController();
        object result = controller.CreateJsonResult(user);

        // Выполняем результат и получаем ответ.
        (string contentType, string body) = await ExecuteResultAsync(result);

        // Проверяем тип содержимого.
        Assert.AreEqual("application/json; charset=utf-8", contentType);

        // Проверяем содержимое JSON.
        Assert.AreEqual("{\"Name\":\"\\u0412\\u0430\\u043D\\u044F\",\"Age\":20}", body);
    }

    // Проверяем, что JSON корректно обрабатывает строковые значения.
    [TestMethod]
    public async Task JsonResult_ReturnsCorrectStringValues()
    {
        // Создаём объект с текстовым значением.
        object user = new { Login = "player123" };

        // Создаём результат.
        TestController controller = new TestController();
        object result = controller.CreateJsonResult(user);

        // Выполняем результат.
        (string contentType, string body) = await ExecuteResultAsync(result);

        // Проверяем, что JSON содержит переданный логин.
        Assert.AreEqual("{\"Login\":\"player123\"}", body);
    }

    // Выполняет результат и возвращает тип содержимого и тело ответа.
    private static async Task<(string ContentType, string Body)> ExecuteResultAsync(object result)
    {
        // Находим свободный порт для локального тестового соединения.
        TcpListener portListener = new TcpListener(IPAddress.Loopback, 0);
        portListener.Start();

        int port = ((IPEndPoint)portListener.LocalEndpoint).Port;
        portListener.Stop();

        // Создаём локальный HTTP-сервер.
        using HttpListener listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();

        // Отправляем запрос к локальному серверу.
        using HttpClient client = new HttpClient();
        Task<HttpResponseMessage> responseTask = client.GetAsync($"http://127.0.0.1:{port}/");

        // Получаем контекст запроса.
        HttpListenerContext context = await listener.GetContextAsync();

        // Находим метод Execute у результата.
        MethodInfo? executeMethod = result.GetType().GetMethod("Execute");

        // Проверяем, что метод существует.
        Assert.IsNotNull(executeMethod);

        // Выполняем результат и получаем байты ответа.
        byte[] bytes = (byte[])executeMethod.Invoke(result, new object[] { context })!;

        // Сохраняем тип содержимого.
        string contentType = context.Response.ContentType;

        // Записываем полученные байты в ответ.
        await context.Response.OutputStream.WriteAsync(bytes);

        // Завершаем ответ.
        context.Response.Close();

        // Получаем ответ от локального сервера.
        using HttpResponseMessage response = await responseTask;

        // Читаем текст ответа.
        string body = await response.Content.ReadAsStringAsync();

        // Останавливаем локальный сервер.
        listener.Stop();

        // Возвращаем тип содержимого и текст ответа.
        return (contentType, body);
    }
}

// Тестовый контроллер, который позволяет вызвать защищённые методы Html и Json.
public class TestController : ControllerBase
{
    // Создаёт HTML-результат.
    public object CreateHtmlResult(string html)
    {
        return Html(html);
    }

    // Создаёт JSON-результат.
    public object CreateJsonResult(object data)
    {
        return Json(data);
    }
}
