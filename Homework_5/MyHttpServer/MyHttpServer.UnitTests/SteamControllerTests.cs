using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Controllers;

namespace MyHttpServer.UnitTests;

[TestClass]
public class SteamControllerTests
{
    [TestMethod]
    public void Send_InvalidEmail_DoesNotThrow()
    {
        // Создаём контроллер.
        SteamController controller = new SteamController();

        // Проверяем, что обработчик самостоятельно перехватывает ошибку.
        controller.Send("неправильный адрес", "герой");
    }

    [TestMethod]
    public void Send_EmptyEmail_DoesNotThrow()
    {
        // Создаём контроллер.
        SteamController controller = new SteamController();

        // Передаём пустой адрес.
        controller.Send("", "герой");
    }
}