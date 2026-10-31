using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Handlers;

namespace MyHttpServer.UnitTests;

// Тестовый обработчик, который будет принимать запрос.
public class TestHandler : Handler
{
    // Счётчик вызовов обработчика.
    public int CallCount { get; private set; }

    // Код ответа, который установит обработчик.
    private readonly int _statusCode;

    public TestHandler(int statusCode)
    {
        _statusCode = statusCode;
    }

    // Реализуем абстрактный метод.
    public override Task HandleRequestAsync(HttpListenerContext context)
    {
        // Увеличиваем количество вызовов.
        CallCount++;

        // Устанавливаем код ответа.
        context.Response.StatusCode = _statusCode;

        // Закрываем ответ.
        context.Response.Close();

        return Task.CompletedTask;
    }
}

// Наследник, через который можно вызвать защищённый NextAsync.
public class TestHandlerWithNext : Handler
{
    // Реализуем обязательный абстрактный метод.
    public override Task HandleRequestAsync(HttpListenerContext context)
    {
        return NextAsync(context);
    }

    // Открываем NextAsync для тестов.
    public Task CallNextAsync(HttpListenerContext context)
    {
        return NextAsync(context);
    }
}

[TestClass]
public class HandlerTests
{
    // Проверяем, что SetNext назначает следующий обработчик.
    [TestMethod]
    public async Task SetNext_AssignsNextHandler()
    {
        // Создаём первый обработчик.
        TestHandlerWithNext firstHandler = new TestHandlerWithNext();

        // Создаём следующий обработчик.
        TestHandler nextHandler = new TestHandler(200);

        // Назначаем следующий обработчик.
        firstHandler.SetNext(nextHandler);

        // Выполняем запрос через первый обработчик.
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://127.0.0.1:8892/");
        listener.Start();

        HttpClient client = new HttpClient();

        try
        {
            Task<HttpListenerContext> contextTask = listener.GetContextAsync();
            Task<HttpResponseMessage> responseTask =
                client.GetAsync("http://127.0.0.1:8892/");

            HttpListenerContext context = await contextTask;

            await firstHandler.CallNextAsync(context);

            HttpResponseMessage response = await responseTask;

            // Проверяем, что следующий обработчик был вызван.
            Assert.AreEqual(1, nextHandler.CallCount);

            // Проверяем код ответа.
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            response.Dispose();
        }
        finally
        {
            listener.Stop();
            client.Dispose();
        }
    }

    // Проверяем, что SetNext заменяет предыдущий обработчик.
    [TestMethod]
    public async Task SetNext_ReplacesPreviousHandler()
    {
        // Создаём первый обработчик цепочки.
        TestHandlerWithNext firstHandler = new TestHandlerWithNext();

        // Создаём два возможных следующих обработчика.
        TestHandler oldHandler = new TestHandler(200);
        TestHandler newHandler = new TestHandler(201);

        // Сначала назначаем старый обработчик.
        firstHandler.SetNext(oldHandler);

        // Затем заменяем его новым.
        firstHandler.SetNext(newHandler);

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://127.0.0.1:8893/");
        listener.Start();

        HttpClient client = new HttpClient();

        try
        {
            Task<HttpListenerContext> contextTask = listener.GetContextAsync();
            Task<HttpResponseMessage> responseTask =
                client.GetAsync("http://127.0.0.1:8893/");

            HttpListenerContext context = await contextTask;

            await firstHandler.CallNextAsync(context);

            HttpResponseMessage response = await responseTask;

            // Старый обработчик не должен вызываться.
            Assert.AreEqual(0, oldHandler.CallCount);

            // Новый обработчик должен быть вызван.
            Assert.AreEqual(1, newHandler.CallCount);

            // Проверяем код ответа нового обработчика.
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            response.Dispose();
        }
        finally
        {
            listener.Stop();
            client.Dispose();
        }
    }

    // Проверяем, что без следующего обработчика возвращается 404.
    [TestMethod]
    public async Task NextAsync_WithoutNextHandler_Returns404()
    {
        // Создаём обработчик без следующего обработчика.
        TestHandlerWithNext handler = new TestHandlerWithNext();

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://127.0.0.1:8894/");
        listener.Start();

        HttpClient client = new HttpClient();

        try
        {
            Task<HttpListenerContext> contextTask = listener.GetContextAsync();
            Task<HttpResponseMessage> responseTask =
                client.GetAsync("http://127.0.0.1:8894/");

            HttpListenerContext context = await contextTask;

            // Вызываем NextAsync.
            await handler.CallNextAsync(context);

            HttpResponseMessage response = await responseTask;

            // Проверяем, что вернулся статус 404.
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            response.Dispose();
        }
        finally
        {
            listener.Stop();
            client.Dispose();
        }
    }
}
