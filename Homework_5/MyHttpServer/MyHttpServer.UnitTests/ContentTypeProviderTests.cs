using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Core;
using System.Reflection;

namespace MyHttpServer.UnitTests;

[TestClass]
public class ContentTypeProviderTests
{
    private Type GetProviderType()
    {
        Assembly assembly = typeof(HttpServer).Assembly;

        Type? providerType = assembly.GetTypes()
            .FirstOrDefault(type => type.Name == "ContentTypeProvider");

        Assert.IsNotNull(providerType);

        return providerType;
    }

    [TestMethod]
    public void GetContentType_HtmlFile_ReturnsHtmlContentType()
    {
        // Arrange
        Type providerType = GetProviderType();

        // Act
        MethodInfo? method = providerType.GetMethod("GetContentType");

        Assert.IsNotNull(method);

        object? result = method.Invoke(null, new object[] { "index.html" });

        // Assert
        Assert.AreEqual("text/html; charset=utf-8", result);
    }

    [TestMethod]
    public void GetContentType_CssFile_ReturnsCssContentType()
    {
        // Arrange
        Type providerType = GetProviderType();
        MethodInfo? method = providerType.GetMethod("GetContentType");

        Assert.IsNotNull(method);

        // Act
        object? result = method.Invoke(null, new object[] { "style.css" });

        // Assert
        Assert.AreEqual("text/css; charset=utf-8", result);
    }

    [TestMethod]
    public void GetContentType_JavaScriptFile_ReturnsJavaScriptContentType()
    {
        // Arrange
        Type providerType = GetProviderType();
        MethodInfo? method = providerType.GetMethod("GetContentType");

        Assert.IsNotNull(method);

        // Act
        object? result = method.Invoke(null, new object[] { "script.js" });

        // Assert
        Assert.AreEqual("text/javascript; charset=utf-8", result);
    }

    [TestMethod]
    public void GetContentType_PngFile_ReturnsPngContentType()
    {
        // Arrange
        Type providerType = GetProviderType();
        MethodInfo? method = providerType.GetMethod("GetContentType");

        Assert.IsNotNull(method);

        // Act
        object? result = method.Invoke(null, new object[] { "image.png" });

        // Assert
        Assert.AreEqual("image/png", result);
    }

    [TestMethod]
    public void GetContentType_IcoFile_ReturnsIconContentType()
    {
        // Arrange
        Type providerType = GetProviderType();
        MethodInfo? method = providerType.GetMethod("GetContentType");

        Assert.IsNotNull(method);

        // Act
        object? result = method.Invoke(null, new object[] { "favicon.ico" });

        // Assert
        Assert.AreEqual("image/x-icon", result);
    }

    [TestMethod]
    public void GetContentType_SvgFile_ReturnsSvgContentType()
    {
        // Arrange
        Type providerType = GetProviderType();
        MethodInfo? method = providerType.GetMethod("GetContentType");

        Assert.IsNotNull(method);

        // Act
        object? result = method.Invoke(null, new object[] { "image.svg" });

        // Assert
        Assert.AreEqual("image/svg+xml", result);
    }

    [TestMethod]
    public void GetContentType_JpgFile_ReturnsJpegContentType()
    {
        // Arrange
        Type providerType = GetProviderType();
        MethodInfo? method = providerType.GetMethod("GetContentType");

        Assert.IsNotNull(method);

        // Act
        object? result = method.Invoke(null, new object[] { "photo.jpg" });

        // Assert
        Assert.AreEqual("image/jpeg", result);
    }

    [TestMethod]
    public void GetContentType_UnknownExtension_ReturnsDefaultContentType()
    {
        // Arrange
        Type providerType = GetProviderType();
        MethodInfo? method = providerType.GetMethod("GetContentType");

        Assert.IsNotNull(method);

        // Act
        object? result = method.Invoke(null, new object[] { "document.unknown" });

        // Assert
        Assert.AreEqual("application/octet-stream", result);
    }
}