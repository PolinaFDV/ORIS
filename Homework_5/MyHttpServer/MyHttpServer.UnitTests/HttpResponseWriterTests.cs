using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Http;

namespace MyHttpServer.UnitTests;

[TestClass]
public class HttpResponseWriterTests
{
    [TestMethod]
    public async Task WriteHtmlAsync_ReturnsHtmlContent()
    {
        // Arrange
        string html = "<h1>Привет!</h1>";

        // Act
        (int statusCode, string contentType, string body) =
            await ExecuteResponseAsync(async response =>
            {
                await HttpResponseWriter.WriteHtmlAsync(response, html);
            });

        // Assert
        Assert.AreEqual(200, statusCode);
        Assert.AreEqual("text/html; charset=utf-8", contentType);
        Assert.AreEqual(html, body);
    }

    [TestMethod]
    public async Task WriteHtmlAsync_CustomStatusCode_ReturnsCorrectStatus()
    {
        // Arrange
        string html = "<h1>Ошибка</h1>";

        // Act
        (int statusCode, string contentType, string body) =
            await ExecuteResponseAsync(async response =>
            {
                await HttpResponseWriter.WriteHtmlAsync(response, html, 404);
            });

        // Assert
        Assert.AreEqual(404, statusCode);
        Assert.AreEqual("text/html; charset=utf-8", contentType);
        Assert.AreEqual(html, body);
    }

    [TestMethod]
    public async Task WriteAsync_ReturnsTextWithCorrectContentType()
    {
        // Arrange
        string text = "Привет, мир!";
        string contentType = "text/plain; charset=utf-8";

        // Act
        (int statusCode, string actualContentType, string body) =
            await ExecuteResponseAsync(async response =>
            {
                await HttpResponseWriter.WriteAsync(
                    response,
                    text,
                    contentType);
            });

        // Assert
        Assert.AreEqual(200, statusCode);
        Assert.AreEqual(contentType, actualContentType);
        Assert.AreEqual(text, body);
    }

    [TestMethod]
    public async Task WriteAsync_CustomStatusCode_ReturnsCorrectStatus()
    {
        // Arrange
        string text = "Ошибка сервера";

        // Act
        (int statusCode, string contentType, string body) =
            await ExecuteResponseAsync(async response =>
            {
                await HttpResponseWriter.WriteAsync(
                    response,
                    text,
                    "text/plain",
                    500);
            });

        // Assert
        Assert.AreEqual(500, statusCode);
        Assert.AreEqual("text/plain", contentType);
        Assert.AreEqual(text, body);
    }

    [TestMethod]
    public async Task WriteBytesAsync_ReturnsCorrectBytes()
    {
        // Arrange
        byte[] expectedBytes = Encoding.UTF8.GetBytes("Тестовые данные");

        // Act
        (int statusCode, string contentType, byte[] actualBytes) =
            await ExecuteBytesResponseAsync(async response =>
            {
                await HttpResponseWriter.WriteBytesAsync(
                    response,
                    expectedBytes,
                    201);
            });

        // Assert
        Assert.AreEqual(201, statusCode);
        CollectionAssert.AreEqual(expectedBytes, actualBytes);
    }

    private static async Task<(int StatusCode, string ContentType, string Body)>
        ExecuteResponseAsync(Func<HttpListenerResponse, Task> writeResponse)
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

        using HttpClient client = new HttpClient();

        // Отправляем GET-запрос.
        Task<HttpResponseMessage> clientTask =
            client.GetAsync($"http://127.0.0.1:{port}/");

        // Получаем запрос.
        HttpListenerContext context = await listener.GetContextAsync();

        // Записываем ответ.
        await writeResponse(context.Response);

        // Получаем ответ клиента.
        HttpResponseMessage clientResponse = await clientTask;

        string body = await clientResponse.Content.ReadAsStringAsync();

        return (
            (int)clientResponse.StatusCode,
            clientResponse.Content.Headers.ContentType?.ToString() ?? "",
            body);
    }

    private static async Task<(int StatusCode, string ContentType, byte[] Body)>
        ExecuteBytesResponseAsync(Func<HttpListenerResponse, Task> writeResponse)
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

        using HttpClient client = new HttpClient();

        // Отправляем GET-запрос.
        Task<HttpResponseMessage> clientTask =
            client.GetAsync($"http://127.0.0.1:{port}/");

        // Получаем запрос.
        HttpListenerContext context = await listener.GetContextAsync();

        // Записываем ответ.
        await writeResponse(context.Response);

        // Получаем ответ клиента.
        HttpResponseMessage clientResponse = await clientTask;

        byte[] body = await clientResponse.Content.ReadAsByteArrayAsync();

        return (
            (int)clientResponse.StatusCode,
            clientResponse.Content.Headers.ContentType?.ToString() ?? "",
            body);
    }
}