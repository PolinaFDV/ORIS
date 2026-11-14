using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Controllers;
using System;

namespace MyHttpServer.UnitTests;

[TestClass]
public class HomeworkControllerTests
{
    [TestMethod]
    public void Send_InvalidEmail_ThrowsArgumentException()
    {
        // Создаём контроллер.
        HomeworkController controller = new HomeworkController();

        // Проверяем вызов метода с неправильным адресом.
        try
        {
            controller.Send("неправильный адрес");

            // Если исключения не было, тест должен завершиться ошибкой.
            Assert.Fail("Ожидалось исключение ArgumentException.");
        }
        catch (ArgumentException ex)
        {
            // Проверяем текст исключения.
            Assert.AreEqual("Некорректный адрес почты.", ex.Message);
        }
    }

    [TestMethod]
    public void Send_EmptyEmail_ThrowsArgumentException()
    {
        // Создаём контроллер.
        HomeworkController controller = new HomeworkController();

        // Проверяем вызов метода с пустым адресом.
        try
        {
            controller.Send("");

            // Если исключения не было, тест должен завершиться ошибкой.
            Assert.Fail("Ожидалось исключение ArgumentException.");
        }
        catch (ArgumentException ex)
        {
            // Проверяем текст исключения.
            Assert.AreEqual("Некорректный адрес почты.", ex.Message);
        }
    }
}