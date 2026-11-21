using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Core;

namespace MiniHttpServer.Framework.UnitTests.Content;

[TestClass]
public class ContentTypeProviderTests
{
    [TestMethod]
    public void GetContentType_HtmlFile_ReturnsCorrectMimeType()
    {
        // Arrange
        string path = "test.html";

        // Act
        string result = ContentTypeProvider.GetContentType(path);

        // Assert
        Assert.AreEqual("text/html; charset=utf-8", result);
    }

    [TestMethod]
    public void GetContentType_CssFile_ReturnsCorrectMimeType()
    {
        // Arrange
        string path = "style.css";

        // Act
        string result = ContentTypeProvider.GetContentType(path);

        // Assert
        Assert.AreEqual("text/css; charset=utf-8", result);
    }

    [TestMethod]
    public void GetContentType_UnknownExtension_ReturnsOctetStream()
    {
        // Arrange
        string path = "archive.zip";

        // Act
        string result = ContentTypeProvider.GetContentType(path);

        // Assert
        Assert.AreEqual("application/octet-stream", result);
    }
}