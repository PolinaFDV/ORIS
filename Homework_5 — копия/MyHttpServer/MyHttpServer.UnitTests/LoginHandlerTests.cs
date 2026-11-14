using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Handlers;

namespace MyHttpServer.UnitTests;

[TestClass]
public class LoginHandlerTests
{
    [TestMethod]
    public async Task HandleLogin_ValidData_ReturnsSuccess()
    {
        // Отправляем обычные логин и пароль.
        var result = await ExecuteRequestAsync(
            "login=Polina&password=12345"
        );

        // Проверяем код ответа.
        Assert.AreEqual(200, result.StatusCode);

        // Проверяем текст ответа.
        Assert.AreEqual(
            "Данные получены. Проверьте консоль сервера.",
            result.ResponseBody
        );
    }

    [TestMethod]
    public async Task HandleLogin_EncodedData_ReturnsSuccess()
    {
        // Отправляем данные с пробелом и специальным символом.
        var result = await ExecuteRequestAsync(
            "login=Полина+Ф&password=пароль%40123"
        );

        // Проверяем, что обработчик успешно принял данные.
        Assert.AreEqual(200, result.StatusCode);

        // Проверяем текст ответа.
        Assert.AreEqual(
            "Данные получены. Проверьте консоль сервера.",
            result.ResponseBody
        );
    }

    [TestMethod]
    public async Task HandleLogin_IncompleteData_ReturnsSuccess()
    {
        // Отправляем неполные и неизвестные данные.
        var result = await ExecuteRequestAsync(
            "login&password=&unknown=value"
        );

        // Проверяем, что обработчик не завершился ошибкой.
        Assert.AreEqual(200, result.StatusCode);

        // Проверяем текст ответа.
        Assert.AreEqual(
            "Данные получены. Проверьте консоль сервера.",
            result.ResponseBody
        );
    }

    private async Task<(int StatusCode, string ResponseBody)>
        ExecuteRequestAsync(string formData)
    {
        // Создаём TCP-слушатель, чтобы найти свободный порт.
        TcpListener portListener = new TcpListener(IPAddress.Loopback, 0);

        portListener.Start();

        int port = ((IPEndPoint)portListener.LocalEndpoint).Port;

        // Освобождаем временно занятый порт.
        portListener.Stop();

        // Создаём локальный HTTP-слушатель.
        using HttpListener listener = new HttpListener();

        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();

        // Создаём HTTP-клиент.
        using HttpClient client = new HttpClient();

        // Отправляем POST-запрос с данными формы.
        Task<HttpResponseMessage> responseTask = client.PostAsync(
            $"http://127.0.0.1:{port}/login",
            new StringContent(
                formData,
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            )
        );

        // Получаем запрос от клиента.
        HttpListenerContext context = await listener.GetContextAsync();

        // Создаём обработчик.
        LoginHandler handler = new LoginHandler();

        // Вызываем непосредственно проверяемый метод.
        await handler.HandleLogin(context);

        // Получаем ответ обработчика.
        HttpResponseMessage response = await responseTask;

        // Читаем текст ответа.
        string responseBody = await response.Content.ReadAsStringAsync();

        // Возвращаем результаты проверки.
        return ((int)response.StatusCode, responseBody);
    }
}
