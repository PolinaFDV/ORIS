using System.Net;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Handlers;

public abstract class Handler
{
    private Handler? _next;

    public void SetNext(Handler next)
    {
        _next = next;
    }

    public abstract Task HandleRequestAsync(HttpListenerContext context);

    protected async Task NextAsync(HttpListenerContext context)
    {
        if (_next != null)
        {
            await _next.HandleRequestAsync(context);
        }
        else
        {
            context.Response.StatusCode = 404;
            context.Response.Close();
        }
    }
}