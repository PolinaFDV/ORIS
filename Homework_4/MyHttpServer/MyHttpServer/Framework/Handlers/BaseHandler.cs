using System.Net;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Handlers
{
    public abstract class BaseHandler
    {
        private BaseHandler? _nextHandler;

        public BaseHandler SetNext(BaseHandler nextHandler)
        {
            _nextHandler = nextHandler;
            return nextHandler;
        }

        public virtual async Task HandleRequestAsync(HttpListenerContext context)
        {
            if (_nextHandler != null)
            {
                await _nextHandler.HandleRequestAsync(context);
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
            }
        }
    }
}