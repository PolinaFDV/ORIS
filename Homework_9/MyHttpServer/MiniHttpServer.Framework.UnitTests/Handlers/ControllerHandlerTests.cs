using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Handlers;
using System.Reflection;

namespace MiniHttpServer.Framework.UnitTests.Handlers;

[TestClass]
public class ControllerHandlerTests
{
    public class TestControllerForRouting
    {
        [MyHttpServer.Framework.Core.Get("users")]
        public void GetUsers() { }

        [MyHttpServer.Framework.Core.Post("users")]
        public void PostUser() { }
    }

    [TestMethod]
    public void MatchesRoute_GetMethod_MatchesCorrectly()
    {
        MethodInfo method = typeof(TestControllerForRouting).GetMethod(nameof(TestControllerForRouting.GetUsers))!;
        bool result = ControllerHandler.MatchesRoute(method, "GET", "users");
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void MatchesRoute_PostMethod_DoesNotMatchGet()
    {
        MethodInfo method = typeof(TestControllerForRouting).GetMethod(nameof(TestControllerForRouting.PostUser))!;
        bool result = ControllerHandler.MatchesRoute(method, "GET", "users");
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ConvertValue_ValidInteger_ReturnsInteger()
    {
        MethodInfo method = typeof(TestControllerForRouting).GetMethod(nameof(TestControllerForRouting.GetUsers))!;
        ParameterInfo[] parameters = method.GetParameters();
        
        ParameterInfo fakeParam = typeof(string).GetMethod("Substring", new[] { typeof(int) })!.GetParameters()[0];
        
        object? result = ControllerHandler.ConvertValue("42", fakeParam);
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ParseUrlEncoded_ValidString_ReturnsDictionary()
    {
        string data = "name=Alex&age=20";
        var result = ControllerHandler.ParseUrlEncoded(data);
        
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Alex", result["name"]);
        Assert.AreEqual("20", result["age"]);
    }

    [TestMethod]
    public void ConvertValue_InvalidInteger_ThrowsArgumentException()
    {
        ParameterInfo fakeParam = typeof(string).GetMethod("Substring", new[] { typeof(int) })!.GetParameters()[0];
        
        Assert.ThrowsExactly<ArgumentException>(() => 
        {
            ControllerHandler.ConvertValue("not_a_number", fakeParam);
        });
    }
}