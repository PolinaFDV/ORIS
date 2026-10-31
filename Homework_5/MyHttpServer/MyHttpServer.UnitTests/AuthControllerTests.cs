using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Controllers;
using MyHttpServer.Framework.HttpResponses;

namespace MyHttpServer.UnitTests;

[TestClass]
public class AuthControllerTests
{
    [TestMethod]
    public void Login_ReturnsHtmlResult()
    {
        // Создаём контроллер.
        AuthController controller = new AuthController();

        // Вызываем метод Login.
        var result = controller.Login();

        // Проверяем, что получен HTML-результат.
        Assert.IsInstanceOfType(result, typeof(HtmlResult));
    }

    [TestMethod]
    public void LoginPost_ValidData_ReturnsHtmlWithFormData()
    {
        // Создаём контроллер.
        AuthController controller = new AuthController();

        // Передаём обычные данные формы.
        string result = controller.Login("Polina", "12345", "test");

        // Проверяем, что в ответе присутствует логин.
        Assert.IsTrue(result.Contains("Polina"));

        // Проверяем, что указана длина пароля.
        Assert.IsTrue(result.Contains("5 символов"));

        // Проверяем источник.
        Assert.IsTrue(result.Contains("test"));
    }

    [TestMethod]
    public void LoginPost_SpecialCharacters_EncodesHtml()
    {
        // Создаём контроллер.
        AuthController controller = new AuthController();

        // Передаём строку с HTML-символами.
        string result = controller.Login("<script>", "123", null);

        // Проверяем, что HTML-символы экранированы.
        Assert.IsTrue(result.Contains("&lt;script&gt;"));

        // Проверяем значение источника по умолчанию.
        Assert.IsTrue(result.Contains("не указан"));

        // Проверяем длину пароля.
        Assert.IsTrue(result.Contains("3 символов"));
    }

    [TestMethod]
    public void HtmlPage_ReturnsHtmlResult()
    {
        // Создаём контроллер.
        AuthController controller = new AuthController();

        // Вызываем метод HtmlPage.
        var result = controller.HtmlPage();

        // Проверяем тип результата.
        Assert.IsInstanceOfType(result, typeof(HtmlResult));
    }

    [TestMethod]
    public void JsonPage_ReturnsJsonResult()
    {
        // Создаём контроллер.
        AuthController controller = new AuthController();

        // Вызываем метод JsonPage.
        var result = controller.JsonPage();

        // Проверяем тип результата.
        Assert.IsInstanceOfType(result, typeof(JsonResult));
    }

    [TestMethod]
    public void JsonPage2_ReturnsJsonResult()
    {
        // Создаём контроллер.
        AuthController controller = new AuthController();

        // Вызываем метод JsonPage2.
        var result = controller.JsonPage2();

        // Проверяем тип результата.
        Assert.IsInstanceOfType(result, typeof(JsonResult));
    }
}
