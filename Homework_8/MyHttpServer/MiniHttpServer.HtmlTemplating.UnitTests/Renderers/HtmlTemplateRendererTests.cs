using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.HtmlTemplating;
using System;
using System.Collections.Generic;
using System.IO;

namespace MiniHttpServer.HtmlTemplating.UnitTests.Renderers;

[TestClass]
public class HtmlTemplateRendererTests
{
    private IHtmlTemplateRenderer _renderer = null!;

    [TestInitialize]
    public void Setup()
    {
        _renderer = new HtmlTemplateRenderer();
    }


    [TestMethod]
    public void RenderFromString_ValidProperty_ReplacesPlaceholder()
    {
        // Arrange
        string template = "<h1>${Name}</h1>";
        var model = new { Name = "Alex" };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("<h1>Alex</h1>", result);
    }

    [TestMethod]
    public void RenderFromString_MultipleProperties_ReplacesAllPlaceholders()
    {
        // Arrange
        string template = "${Name} is ${Age} years old.";
        var model = new { Name = "Alex", Age = 20 };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("Alex is 20 years old.", result);
    }

    [TestMethod]
    public void RenderFromString_RepeatedProperty_ReplacesAllInstances()
    {
        // Arrange
        string template = "${Name} ${Name} ${Name}";
        var model = new { Name = "A" };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("A A A", result);
    }

    [TestMethod]
    public void RenderFromString_NestedProperty_ReplacesCorrectly()
    {
        // Arrange
        string template = "User: ${User.Name}, Email: ${User.Email}";
        var model = new { User = new { Name = "Alex", Email = "alex@test.com" } };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("User: Alex, Email: alex@test.com", result);
    }

    [TestMethod]
    public void RenderFromString_EmptyTemplate_ReturnsEmptyString()
    {
        // Arrange
        string template = "";
        var model = new { Name = "Alex" };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void RenderFromString_HtmlSpecialChars_EncodesHtml()
    {
        // Arrange
        string template = "${Name}";
        var model = new { Name = "<script>" };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("&lt;script&gt;", result);
    }


    [TestMethod]
    public void RenderFromString_TrueCondition_RendersIfBlock()
    {
        // Arrange
        string template = "$if(IsActive)Active$endif";
        var model = new { IsActive = true };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("Active", result);
    }

    [TestMethod]
    public void RenderFromString_FalseCondition_RendersNothing()
    {
        // Arrange
        string template = "$if(IsActive)Active$endif";
        var model = new { IsActive = false };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void RenderFromString_IfElse_RendersCorrectBranch()
    {
        // Arrange
        string template = "$if(IsActive)Yes$else No $endif";
        
        // Act 1
        string resultTrue = _renderer.RenderFromString(template, new { IsActive = true });
        // Act 2
        string resultFalse = _renderer.RenderFromString(template, new { IsActive = false });

        // Assert
        Assert.AreEqual("Yes", resultTrue.Trim());
        Assert.AreEqual("No", resultFalse.Trim());
    }

    [TestMethod]
    public void RenderFromString_NestedIf_RendersCorrectly()
    {
        // Arrange
        string template = "$if(A)$if(B)AB$endif$endif";
        var model = new { A = true, B = true };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("AB", result);
    }


    [TestMethod]
    public void RenderFromString_Foreach_RendersAllItems()
    {
        // Arrange
        string template = "$foreach(var item in Items)${item}$endfor";
        var model = new { Items = new List<string> { "A", "B", "C" } };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("ABC", result);
    }

    [TestMethod]
    public void RenderFromString_EmptyCollection_ReturnsNoItems()
    {
        // Arrange
        string template = "<ul>$foreach(var item in Items)<li>${item}</li>$endfor</ul>";
        var model = new { Items = new List<string>() };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("<ul></ul>", result);
    }

    [TestMethod]
    public void RenderFromString_ForeachWithProperties_RendersCorrectly()
    {
        // Arrange
        string template = "$foreach(var user in Users)${user.Name}$endfor";
        var model = new { Users = new[] { new { Name = "A" }, new { Name = "B" } } };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("AB", result);
    }

    [TestMethod]
    public void RenderFromString_NestedForeach_RendersCorrectly()
    {
        // Arrange
        string template = "$foreach(var group in Groups)$foreach(var item in group.Items)${item}$endfor$endfor";
        var model = new { Groups = new[] { new { Items = new[] { "1", "2" } }, new { Items = new[] { "3" } } } };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("123", result);
    }

    [TestMethod]
    public void RenderFromString_IfInsideForeach_RendersCorrectly()
    {
        // Arrange
        string template = "$foreach(var user in Users)$if(user.Active)${user.Name}$endif$endfor";
        var model = new { Users = new[] { new { Name = "A", Active = true }, new { Name = "B", Active = false }, new { Name = "C", Active = true } } };

        // Act
        string result = _renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("AC", result);
    }


    [TestMethod]
    public void RenderFromString_UnclosedIf_ThrowsException()
    {
        // Arrange
        string template = "$if(IsActive)Active";

        // Act & Assert
        Assert.ThrowsExactly<Exception>(() => _renderer.RenderFromString(template, new { IsActive = true }));
    }

    [TestMethod]
    public void RenderFromString_UnclosedForeach_ThrowsException()
    {
        // Arrange
        string template = "$foreach(var item in Items)${item}";

        // Act & Assert
        Assert.ThrowsExactly<Exception>(() => _renderer.RenderFromString(template, new { Items = new[] { 1 } }));
    }


    [TestMethod]
    public void RenderFromFile_ValidFile_ReturnsRenderedHtml()
    {
        // Arrange
        string tempPath = Path.GetTempFileName();
        File.WriteAllText(tempPath, "<p>${Name}</p>");
        var model = new { Name = "FileTest" };

        try
        {
            // Act
            string result = _renderer.RenderFromFile(tempPath, model);

            // Assert
            Assert.AreEqual("<p>FileTest</p>", result);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [TestMethod]
    public void RenderToFile_ValidData_SavesRenderedFile()
    {
        // Arrange
        string inputPath = Path.GetTempFileName();
        string outputPath = Path.GetTempFileName();
        File.WriteAllText(inputPath, "Hello ${Name}");
        var model = new { Name = "World" };

        try
        {
            // Act
            _renderer.RenderToFile(inputPath, outputPath, model);

            // Assert
            string savedContent = File.ReadAllText(outputPath);
            Assert.AreEqual("Hello World", savedContent);
        }
        finally
        {
            File.Delete(inputPath);
            File.Delete(outputPath);
        }
    }
}