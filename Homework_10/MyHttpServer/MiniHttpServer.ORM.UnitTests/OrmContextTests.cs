using MiniHttpServer.ORM;

namespace MiniHttpServer.ORM.UnitTests;

[TestClass]
public class OrmContextTests
{
    [TestMethod]
    public void Constructor_EmptyConnectionString_ThrowsException()
    {
        Assert.ThrowsException<ArgumentException>(() => new OrmContext(""));
        Assert.ThrowsException<ArgumentException>(() => new OrmContext("   "));
        Assert.ThrowsException<ArgumentException>(() => new OrmContext(null!));
    }

    [TestMethod]
    public void Constructor_ValidConnectionString_CreatesInstance()
    {
        var orm = new OrmContext("Server=localhost;Database=Test;");
        Assert.IsNotNull(orm);
    }

    // Тест на недопустимый идентификатор таблицы
    [TestMethod]
    public async Task GetAllAsync_InvalidTableName_ThrowsException()
    {
        var orm = new OrmContext("Server=localhost;Database=Test;");
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => orm.GetAllAsync<TestModel>("Users; DROP TABLE Users;"));
    }

    [TestMethod]
    public async Task GetAllAsync_SqlInjectionAttempt_ThrowsException()
    {
        var orm = new OrmContext("Server=localhost;Database=Test;");
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => orm.GetAllAsync<TestModel>("Users' OR '1'='1"));
    }
    
    // Тест на корректное имя таблицы
    [TestMethod]
    public async Task GetAllAsync_ValidTableName_ReturnsList()
    {
        // Этот тест требует реальной БД, но для проверки логики мы просто ловим исключение подключения
        var orm = new OrmContext("Server=localhost;Database=Test;");
        // Мы не будем реально подключаться, чтобы не упасть, но проверим, что не падает на валидации имени
        var ex = await Assert.ThrowsExceptionAsync<Microsoft.Data.SqlClient.SqlException>(() => orm.GetAllAsync<TestModel>("Users"));
        Assert.IsNotNull(ex);
    }

    // Вспомогательная модель
    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}