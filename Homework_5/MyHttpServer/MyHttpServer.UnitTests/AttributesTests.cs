using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Core;
using System.Reflection;

namespace MyHttpServer.UnitTests;

[TestClass]
public class AttributesTests
{
    [TestMethod]
    public void HttpControllerAttribute_Constructor_SavesRoute()
    {
        // Arrange
        string route = "auth";

        // Act
        HttpControllerAttribute attribute =
            new HttpControllerAttribute(route);

        // Assert
        Assert.AreEqual("auth", attribute.Route);
    }

    [TestMethod]
    public void HttpControllerAttribute_Constructor_RemovesSlashes()
    {
        // Arrange
        string route = "/auth/";

        // Act
        HttpControllerAttribute attribute =
            new HttpControllerAttribute(route);

        // Assert
        Assert.AreEqual("auth", attribute.Route);
    }

    [TestMethod]
    public void HttpControllerAttribute_Constructor_EmptyRoute_ReturnsEmptyString()
    {
        // Arrange
        string route = "";

        // Act
        HttpControllerAttribute attribute =
            new HttpControllerAttribute(route);

        // Assert
        Assert.AreEqual("", attribute.Route);
    }

    [TestMethod]
    public void GetAttribute_Constructor_SavesGetMethod()
    {
        // Arrange
        string path = "login";

        // Act
        GetAttribute attribute = new GetAttribute(path);

        // Assert
        Assert.AreEqual("GET", attribute.Method);
        Assert.AreEqual("login", attribute.Path);
    }

    [TestMethod]
    public void GetAttribute_Constructor_SavesPath()
    {
        // Arrange
        string path = "profile";

        // Act
        GetAttribute attribute = new GetAttribute(path);

        // Assert
        Assert.AreEqual("profile", attribute.Path);
    }

    [TestMethod]
    public void PostAttribute_Constructor_SavesPostMethod()
    {
        // Arrange
        string path = "login";

        // Act
        PostAttribute attribute = new PostAttribute(path);

        // Assert
        Assert.AreEqual("POST", attribute.Method);
        Assert.AreEqual("login", attribute.Path);
    }

    [TestMethod]
    public void PostAttribute_Constructor_SavesPath()
    {
        // Arrange
        string path = "send";

        // Act
        PostAttribute attribute = new PostAttribute(path);

        // Assert
        Assert.AreEqual("send", attribute.Path);
    }

    [TestMethod]
    public void FromQueryAttribute_CanBeAppliedToParameter()
    {
        // Arrange
        MethodInfo method = typeof(AttributeTestController)
            .GetMethod(nameof(AttributeTestController.QueryMethod))!;

        // Act
        ParameterInfo parameter = method.GetParameters()[0];

        // Assert
        Assert.IsNotNull(
            parameter.GetCustomAttribute<FromQueryAttribute>());
    }

    [TestMethod]
    public void FromFormAttribute_CanBeAppliedToParameter()
    {
        // Arrange
        MethodInfo method = typeof(AttributeTestController)
            .GetMethod(nameof(AttributeTestController.FormMethod))!;

        // Act
        ParameterInfo parameter = method.GetParameters()[0];

        // Assert
        Assert.IsNotNull(
            parameter.GetCustomAttribute<FromFormAttribute>());
    }

    [TestMethod]
    public void FromQueryAttribute_DoesNotApplyToClass()
    {
        // Arrange
        AttributeUsageAttribute? usage =
            typeof(FromQueryAttribute)
                .GetCustomAttribute<AttributeUsageAttribute>();

        // Act
        Assert.IsNotNull(usage);

        bool canApplyToClass =
            usage.ValidOn.HasFlag(AttributeTargets.Class);

        // Assert
        Assert.IsFalse(canApplyToClass);
    }

    [TestMethod]
    public void FromFormAttribute_DoesNotApplyToClass()
    {
        // Arrange
        AttributeUsageAttribute? usage =
            typeof(FromFormAttribute)
                .GetCustomAttribute<AttributeUsageAttribute>();

        // Act
        bool canApplyToClass =
            usage!.ValidOn.HasFlag(AttributeTargets.Class);

        // Assert
        Assert.IsFalse(canApplyToClass);
    }
}

// Вспомогательный класс нужен только для проверки атрибутов параметров.
public class AttributeTestController
{
    public void QueryMethod([FromQuery] string value)
    {
    }

    public void FormMethod([FromForm] string value)
    {
    }
}