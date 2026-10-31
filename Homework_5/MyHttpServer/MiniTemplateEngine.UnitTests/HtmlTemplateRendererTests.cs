namespace MiniTemplateEngine.UnitTests;

[TestClass]
public class HtmlTemplateRendererTests
{
    [TestMethod]
    public void RendererFromString_Possitive_ReturnValid()
    {
        // Arrange
        string template = "<p>Имя: ${Name}</p>" +
                           "<p>Группа: ${Group}</p>" +
                           "<p>Возраст: ${Age}</p>" +
                           "<p>Пол: ${Sex}</p>";
        var user = new { Name = "Ваня", Group = "11-541", Age = 22, Sex = true };
        
        string expectedStr = "<p>Имя: Ваня</p>" +
                             "<p>Группа: 11-541</p>" +
                             "<p>Возраст: 22</p>" +
                             "<p>Пол: True</p>";
        
        var serv = new HtmlTemplateRenderer();
        
        // Act
        string result = serv.RenderFromString(template, user);
        
        // Assert
        Assert.AreEqual(expectedStr, result);
    }
    
    [TestMethod]
    public void RendererFromString_Possitive_ReturnValid2()
    {
        // Arrange
        string templateBook = "<p>Название книги: ${Name}</p> " +
                              "<p>Год издания: ${Year}</p> " +
                              "<p>Автор: ${Author}</p>";

        var book = new {Name = "Солярис", Year = "1961", Author = "Станислав"};
        
        string expectedStr = "<p>Название книги: Солярис</p> " +
                             "<p>Год издания: 1961</p> " +
                             "<p>Автор: Станислав</p>";
        
        var serv = new HtmlTemplateRenderer();
        
        // Act
        string result = serv.RenderFromString(templateBook, book);
        
        // Assert
        Assert.AreEqual(expectedStr, result);
    }
    
    [TestMethod]
    public void RendererFromString_WhenObjectIsNull_ReturnAException()
    {
        // Arrange
        string templateBook = "<p>Название книги: ${Name}</p> " +
                              "<p>Год издания: ${Year}</p> " +
                              "<p>Автор: ${Author}</p>";

        var book = new {Name = "Солярис", Year = "1961", Author = "Станислав"};
        
        string expectedStr = "<p>Название книги: Солярис</p> " +
                             "<p>Год издания: 1961</p> " +
                             "<p>Автор: Станислав</p>";
        
        var serv = new HtmlTemplateRenderer();
        
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => serv.RenderFromString(null, book));
    }
}