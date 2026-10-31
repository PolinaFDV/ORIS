
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework;
using MyHttpServer.Framework.HttpResponses;

namespace MyHttpServer.UnitTests;

// Создаём класс-наследник, чтобы получить доступ к protected-методам.
public class TestControllerBase : ControllerBase
{
    // Делаем Context доступным для тестов.
    public HttpListenerContext GetContext()
    {
        return Context;
    }

    // Делаем метод Html доступным для тестов.
    public IHttpResponseTypeResult GetHtml(string html)
    {
        return Html(html);
    }

    // Делаем метод Json доступным для тестов.
    public IHttpResponseTypeResult GetJson(object data)
    {
        return Json(data);
    }
}

[TestClass]
public class ControllerBaseTests
{
    // Проверяем, что SetContext сохраняет переданный контекст.
    [TestMethod]
    public void SetContext_SavesContext()
    {
        // Создаём тестовый контроллер.
        TestControllerBase controller = new TestControllerBase();

        // Создаём HttpListener.
        HttpListener listener = new HttpListener();

        // Добавляем адрес, по которому будет работать listener.
        listener.Prefixes.Add("http://127.0.0.1:8891/");

        // Запускаем listener.
        listener.Start();

        // Создаём запрос.
        HttpClient client = new HttpClient();
        Task<HttpResponseMessage> requestTask =
            client.GetAsync("http://127.0.0.1:8891/");

        // Получаем контекст входящего запроса.
        HttpListenerContext context = listener.GetContext();

        // Передаём контекст контроллеру.
        controller.SetContext(context);

        // Проверяем, что сохранился именно тот же объект.
        Assert.AreSame(context, controller.GetContext());

        // Закрываем ответ и освобождаем ресурсы.
        context.Response.Close();
        listener.Stop();
        client.Dispose();
    }

    // Проверяем, что Html возвращает HtmlResult.
    [TestMethod]
    public void Html_ReturnsHtmlResult()
    {
        // Создаём тестовый контроллер.
        TestControllerBase controller = new TestControllerBase();

        // Вызываем метод Html.
        IHttpResponseTypeResult result = controller.GetHtml("<h1>Hello</h1>");

        // Проверяем тип результата.
        Assert.IsInstanceOfType(result, typeof(HtmlResult));
    }

    // Проверяем, что Json возвращает JsonResult.
    [TestMethod]
    public void Json_ReturnsJsonResult()
    {
        // Создаём тестовый контроллер.
        TestControllerBase controller = new TestControllerBase();

        // Передаём объект в метод Json.
        object data = new { Name = "Ваня", Age = 20 };

        // Получаем результат.
        IHttpResponseTypeResult result = controller.GetJson(data);

        // Проверяем тип результата.
        Assert.IsInstanceOfType(result, typeof(JsonResult));
    }
}
