using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Authentication;
using System;
using System.Reflection;

namespace MyHttpServer.Framework.UnitTests.Authentication;

[TestClass]
public class UserSessionServiceTests
{
    [TestMethod]
    public void CreateSession_ValidUser_ReturnsUniqueId()
    {
        var service = new UserSessionService();
        string id1 = service.CreateSession(1);
        string id2 = service.CreateSession(2);
        Assert.AreNotEqual(id1, id2);
    }

    [TestMethod]
    public void GetUserId_ValidSession_ReturnsUserId()
    {
        var service = new UserSessionService();
        string sessionId = service.CreateSession(123);
        int? userId = service.GetUserId(sessionId);
        Assert.AreEqual(123, userId);
    }

    [TestMethod]
    public void GetUserId_NullOrEmptySession_ReturnsNull()
    {
        var service = new UserSessionService();
        Assert.IsNull(service.GetUserId(null));
        Assert.IsNull(service.GetUserId(""));
        Assert.IsNull(service.GetUserId("   "));
    }

    [TestMethod]
    public void GetUserId_UnknownSession_ReturnsNull()
    {
        var service = new UserSessionService();
        Assert.IsNull(service.GetUserId("unknown_session_id"));
    }

    [TestMethod]
    public void GetUserId_ExpiredSession_ReturnsNull()
    {
        var service = new UserSessionService();
        string sessionId = service.CreateSession(1);

        var sessionsField = typeof(UserSessionService).GetField("_sessions", BindingFlags.NonPublic | BindingFlags.Instance);
        var sessions = sessionsField!.GetValue(service) as System.Collections.IDictionary;

        var sessionInfoType = typeof(UserSessionService).GetNestedType("SessionInfo", BindingFlags.NonPublic);
        
        var expiredSession = Activator.CreateInstance(sessionInfoType!, 1, DateTimeOffset.UtcNow.AddMinutes(-1));
        
        sessions![sessionId] = expiredSession;

        Assert.IsNull(service.GetUserId(sessionId));
    }

    [TestMethod]
    public void RemoveSession_ValidSession_RemovesCorrectly()
    {
        var service = new UserSessionService();
        string sessionId = service.CreateSession(1);
        service.RemoveSession(sessionId);
        Assert.IsNull(service.GetUserId(sessionId));
    }

    [TestMethod]
    public void RemoveSession_NullOrEmpty_DoesNotThrow()
    {
        var service = new UserSessionService();
        service.RemoveSession(null);
        service.RemoveSession("");
    }

    [TestMethod]
    public void AuthorizeAttribute_NotPresent_ReturnsFalse()
    {
        var method = typeof(UserSessionServiceTests).GetMethod(nameof(TestMethodWithoutAttribute));
        bool hasAttribute = method!.IsDefined(typeof(MyHttpServer.Framework.Attributes.AuthorizeAttribute), true);
        Assert.IsFalse(hasAttribute);
    }

    [TestMethod]
    public void AuthorizeAttribute_Present_ReturnsTrue()
    {
        var method = typeof(UserSessionServiceTests).GetMethod(nameof(TestMethodWithAttribute));
        bool hasAttribute = method!.IsDefined(typeof(MyHttpServer.Framework.Attributes.AuthorizeAttribute), true);
        Assert.IsTrue(hasAttribute);
    }

    public void TestMethodWithoutAttribute() { }

    [MyHttpServer.Framework.Attributes.Authorize]
    public void TestMethodWithAttribute() { }
}