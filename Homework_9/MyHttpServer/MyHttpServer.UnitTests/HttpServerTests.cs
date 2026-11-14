using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Core;

namespace MyHttpServer.UnitTests;

[TestClass]
public class HttpServerTests
{
    [TestMethod]
    public void Stop_WhenServerIsNotStarted_DoesNotThrow()
    {
        // Создаём сервер, но не запускаем его.
        HttpServer server = new HttpServer();

        // Проверяем, что остановка не вызывает исключение.
        server.Stop();
    }

    [TestMethod]
    public void Start_WhenPortIsOccupied_ThrowsException()
    {
        // Создаём TCP-слушатель.
        TcpListener listener = new TcpListener(IPAddress.Loopback, 8888);

        // Занимаем порт 8888.
        listener.Start();

        try
        {
            // Создаём HTTP-сервер.
            HttpServer server = new HttpServer();

            // Переменная показывает, возникла ли ожидаемая ошибка.
            bool exceptionThrown = false;

            try
            {
                // Пытаемся запустить сервер на занятом порту.
                server.Start();
            }
            catch (HttpListenerException)
            {
                // Если ошибка возникла, меняем значение переменной.
                exceptionThrown = true;
            }

            // Проверяем, что ошибка действительно возникла.
            Assert.IsTrue(exceptionThrown);
        }
        finally
        {
            // Освобождаем порт после теста.
            listener.Stop();
        }
    }

    [TestMethod]
    public void Stop_WhenServerIsStarted_StopsServer()
    {
        // Создаём сервер.
        HttpServer server = new HttpServer();

        // Запускаем сервер.
        server.Start();

        // Останавливаем сервер.
        server.Stop();

        // Повторная остановка не должна вызывать ошибку.
        server.Stop();
    }
}