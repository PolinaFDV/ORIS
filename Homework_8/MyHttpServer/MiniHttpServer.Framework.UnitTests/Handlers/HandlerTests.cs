using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Handlers;
using System.Net;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework.UnitTests.Handlers;

internal class TestHandler : Handler
{
    public bool WasHandled { get; private set; }

    public override Task HandleRequestAsync(HttpListenerContext context)
    {
        WasHandled = true;
        return Task.CompletedTask;
    }
}

[TestClass]
public class HandlerTests
{
    [TestMethod]
    public async Task Handler_CallsNextInChain_WhenNotHandled()
    {
        // Arrange
        var firstHandler = new TestHandler();
        var secondHandler = new TestHandler();
        firstHandler.SetNext(secondHandler);
        
        HttpListenerContext fakeContext = null!; 

        // Act
        await firstHandler.HandleRequestAsync(fakeContext);

        // Assert
        Assert.IsTrue(firstHandler.WasHandled);
        Assert.IsFalse(secondHandler.WasHandled); 
        
    }
}