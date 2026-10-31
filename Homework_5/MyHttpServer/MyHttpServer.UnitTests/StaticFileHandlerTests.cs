using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Handlers;

namespace MyHttpServer.UnitTests;

// Обработчик для проверки передачи запроса дальше.
public class StaticTestNextHandler : Handler
{
    public int CallCount { get; private set; }

    public override Task HandleRequestAsync(HttpListenerContext context)
    {
        // Запоминаем, что обработчик был вызван.
        CallCount++;

        // Устанавливаем статус 202.
        context.Response.StatusCode = 202;

        // Закрываем ответ.
        context.Response.Close();

        return Task.CompletedTask;
    }
}

[TestClass]
public class StaticFileHandlerTests
{
    // Папка static рядом с тестовой программой.
    private string _staticFolder = "";

    [TestInitialize]
    public void SetUp()
    {
        // Получаем путь к папке static.
        _staticFolder = Path.Combine(AppContext.BaseDirectory, "static");

        // Создаём папку, если её ещё нет.
        Directory.CreateDirectory(_staticFolder);
    }

    // Проверяем загрузку существующего файла.
    [TestMethod]
    public async Task HandleRequestAsync_ExistingFile_ReturnsFile()
    {
        // Создаём тестовый файл.
        string filePath = Path.Combine(_staticFolder, "test-file.txt");
        await File.WriteAllTextAsync(filePath, "Hello from test");

        try
        {
            // Создаём обработчик.
            StaticFileHandler handler = new StaticFileHandler();

            // Отправляем GET-запрос.
            var result = await ExecuteRequestAsync(handler, "GET", "/test-file.txt");

            // Проверяем успешный статус.
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

            // Проверяем содержимое файла.
            Assert.AreEqual("Hello from test", result.Body);

            // Проверяем тип содержимого.
            Assert.AreEqual("application/octet-stream", result.ContentType);
        }
        finally
        {
            // Удаляем тестовый файл.
            File.Delete(filePath);
        }
    }

    // Проверяем, что адрес "/" открывает login.html.
    [TestMethod]
    public async Task HandleRequestAsync_RootPath_ReturnsLoginPage()
    {
        string filePath = Path.Combine(_staticFolder, "login.html");

        // Сохраняем старый файл, если он существует.
        byte[]? oldFile = null;

        if (File.Exists(filePath))
        {
            oldFile = await File.ReadAllBytesAsync(filePath);
        }

        try
        {
            // Создаём содержимое тестовой страницы.
            await File.WriteAllTextAsync(filePath, "<h1>Test login</h1>");

            StaticFileHandler handler = new StaticFileHandler();

            // Обращаемся к корневому адресу.
            var result = await ExecuteRequestAsync(handler, "GET", "/");

            // Проверяем, что открылась страница входа.
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual("<h1>Test login</h1>", result.Body);
            Assert.AreEqual("text/html; charset=utf-8", result.ContentType);
        }
        finally
        {
            // Восстанавливаем исходный login.html.
            if (oldFile != null)
            {
                await File.WriteAllBytesAsync(filePath, oldFile);
            }
            else
            {
                File.Delete(filePath);
            }
        }
    }

    // Проверяем передачу запроса следующему обработчику.
    [TestMethod]
    public async Task HandleRequestAsync_NotGetMethod_CallsNextHandler()
    {
        StaticFileHandler handler = new StaticFileHandler();

        // Создаём следующий обработчик.
        StaticTestNextHandler nextHandler = new StaticTestNextHandler();

        // Добавляем его в цепочку.
        handler.SetNext(nextHandler);

        // Отправляем POST-запрос.
        var result = await ExecuteRequestAsync(handler, "POST", "/test-file.txt");

        // Проверяем, что следующий обработчик был вызван.
        Assert.AreEqual(1, nextHandler.CallCount);

        // Проверяем статус, который установил следующий обработчик.
        Assert.AreEqual((HttpStatusCode)202, result.StatusCode);
    }

    // Проверяем ответ, если запрошенного файла нет.
    [TestMethod]
    public async Task HandleRequestAsync_FileNotFound_Returns404()
    {
        StaticFileHandler handler = new StaticFileHandler();

        // Запрашиваем файл, которого не существует.
        var result = await ExecuteRequestAsync(
            handler,
            "GET",
            "/file-that-does-not-exist-12345.txt");

        // Проверяем статус 404.
        Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
    }

    // Вспомогательный метод для отправки локального запроса.
    private async Task<(HttpStatusCode StatusCode, string Body, string ContentType)>
        ExecuteRequestAsync(StaticFileHandler handler, string method, string path)
    {
        // Создаём TCP-порт, который пока свободен.
        TcpListener portListener = new TcpListener(
            IPAddress.Loopback, 0);

        portListener.Start();

        // Получаем номер свободного порта.
        int port = ((IPEndPoint)portListener.LocalEndpoint).Port;

        // Освобождаем порт.
        portListener.Stop();

        // Создаём адрес для HttpListener.
        string address = $"http://127.0.0.1:{port}/";

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(address);
        listener.Start();

        HttpClient client = new HttpClient();

        try
        {
            // Ожидаем входящий запрос.
            Task<HttpListenerContext> contextTask = listener.GetContextAsync();

            // Создаём запрос.
            HttpRequestMessage request = new HttpRequestMessage(
                new HttpMethod(method),
                address.TrimEnd('/') + path);

            // Отправляем запрос.
            Task<HttpResponseMessage> responseTask = client.SendAsync(request);

            // Получаем контекст.
            HttpListenerContext context = await contextTask;

            // Передаём запрос обработчику.
            await handler.HandleRequestAsync(context);

            // Получаем ответ.
            HttpResponseMessage response = await responseTask;

            // Читаем содержимое.
            string body = await response.Content.ReadAsStringAsync();

            // Получаем тип содержимого.
            string contentType =
                response.Content.Headers.ContentType?.ToString() ?? "";

            // Сохраняем результат.
            var result = (response.StatusCode, body, contentType);

            response.Dispose();
            request.Dispose();

            return result;
        }
        finally
        {
            listener.Stop();
            client.Dispose();
        }
    }
}