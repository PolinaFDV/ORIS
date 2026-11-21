using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.ORM;

namespace MiniHttpServer.ORM.UnitTests;

public class TestUser { public int Id { get; set; } public string Name { get; set; } = ""; public int Age { get; set; } }
public class TestProduct { public int Id { get; set; } public string Name { get; set; } = ""; public decimal Price { get; set; } }

[TestClass]
public class OrmContextTests
{
    [TestMethod]
    public void EscapeIdentifier_ValidName_ReturnsQuotedName()
    {
        Assert.AreEqual("\"Users\"", SqlBuilder.EscapeIdentifier("Users"));
    }

    [TestMethod]
    public void EscapeIdentifier_InvalidName_ThrowsException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => SqlBuilder.EscapeIdentifier("Users; DROP"));
    }

    [TestMethod]
    public void BuildInsert_ValidUser_ReturnsCorrectSql()
    {
        var user = new TestUser { Name = "Alex", Age = 20 };
        var (sql, parameters) = SqlBuilder.BuildInsert("Users", user);

        StringAssert.Contains(sql, "INSERT INTO \"Users\"");
        StringAssert.Contains(sql, "\"Name\", \"Age\"");
        StringAssert.Contains(sql, "@Name, @Age");
        Assert.IsTrue(parameters.ContainsKey("@Name"));
        Assert.IsFalse(parameters.ContainsKey("@Id"));
    }

    [TestMethod]
    public void BuildUpdate_ValidUser_ExcludesIdFromSet()
    {
        var user = new TestUser { Name = "Alex", Age = 21 };
        var (sql, parameters) = SqlBuilder.BuildUpdate("Users", 1, user);

        StringAssert.Contains(sql, "UPDATE \"Users\" SET");
        StringAssert.Contains(sql, "\"Name\" = @Name");
        StringAssert.Contains(sql, "WHERE \"Id\" = @Id");
        Assert.AreEqual(1, parameters["@Id"]);
    }

    [TestMethod]
    public void BuildDelete_ValidId_ReturnsCorrectSql()
    {
        var (sql, parameters) = SqlBuilder.BuildDelete("Users", 5);
        Assert.AreEqual("DELETE FROM \"Users\" WHERE \"Id\" = @Id;", sql);
        Assert.AreEqual(5, parameters["@Id"]);
    }

    [TestMethod]
    public void BuildInsert_ProductModel_WorksWithDifferentTypes()
    {
        var product = new TestProduct { Name = "Laptop", Price = 1200.50m };
        var (sql, parameters) = SqlBuilder.BuildInsert("Products", product);
        StringAssert.Contains(sql, "INSERT INTO \"Products\"");
        Assert.IsTrue(parameters.ContainsKey("@Price"));
    }
}