using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Handlers;
using MyHttpServer.Framework.Authentication;
using System.Net;
using System.Reflection;

namespace MiniHttpServer.Framework.UnitTests.Handlers;

public class TestControllerWithoutAuth
{
    public void PublicMethod() { }

    [Authorize]
    public void ProtectedMethod() { }
}

[TestClass]
public class ControllerHandlerAuthTests
{
    private HttpListener _listener = null!;
    private HttpListenerContext _context = null!;
    private ControllerHandler _handler = null!;
    private UserSessionService _sessionService = null!;

    [TestInitialize]
    public void Setup()
    {
        _sessionService = new UserSessionService();
        _handler = new ControllerHandler(_sessionService);

        _listener = new HttpListener();
        _listener.Prefixes.Add("http://127.0.0.1:8889/");
        _listener.Start();

        var client = new HttpClient();
        var task = client.GetAsync("http://127.0.0.1:8889/");
        _context = _listener.GetContext();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _listener.Stop();
        _listener.Close();
    }

    [TestMethod]
    public void IsAuthorized_NoAuthorizeAttribute_ReturnsTrue()
    {
        var method = typeof(TestControllerWithoutAuth).GetMethod(nameof(TestControllerWithoutAuth.PublicMethod))!;
        
        bool result = _handler.IsAuthorized(_context, typeof(TestControllerWithoutAuth), method);
        
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsAuthorized_WithAuthorizeAttributeButNoSession_ReturnsFalse()
    {
        var method = typeof(TestControllerWithoutAuth).GetMethod(nameof(TestControllerWithoutAuth.ProtectedMethod))!;
        
        bool result = _handler.IsAuthorized(_context, typeof(TestControllerWithoutAuth), method);
        
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsAuthorized_WithAuthorizeAttributeAndValidSession_ReturnsTrue()
    {
        var method = typeof(TestControllerWithoutAuth).GetMethod(nameof(TestControllerWithoutAuth.ProtectedMethod))!;
        
        string sessionId = _sessionService.CreateSession(1);
        
        _context.Request.Cookies.Add(new Cookie("sessionId", sessionId));
        
        bool result = _handler.IsAuthorized(_context, typeof(TestControllerWithoutAuth), method);
        
        Assert.IsTrue(result);
    }
}