using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.HtmlTemplating;
using MyHttpServer.Framework.Results;
using System.IO;

namespace MiniHttpServer.Framework.UnitTests.Results;

internal class FakeTemplateRenderer : IHtmlTemplateRenderer
{
    public string RenderFromString(string htmlTemplate, object dataModel) 
        => $"Rendered: {htmlTemplate}";
        
    public string RenderFromFile(string filePath, object dataModel) 
        => $"Rendered file: {Path.GetFileName(filePath)}";
        
    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel) 
        => "Rendered to file";
}

[TestClass]
public class PageResultTests
{
    [TestMethod]
    public void PageResult_WithFakeRenderer_ReturnsRenderedHtml()
    {
        // Arrange
        var fakeRenderer = new FakeTemplateRenderer();
        var model = new { Name = "Alex" };
        
        var pageResult = new PageResult("User/Profile.html", model, renderer: fakeRenderer);
        
    }
}