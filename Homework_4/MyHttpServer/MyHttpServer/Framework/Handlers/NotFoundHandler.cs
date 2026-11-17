using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Handlers
{
    public class NotFoundHandler : BaseHandler
    {
        public override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var response = context.Response;
            
            response.StatusCode = 404;
            response.ContentType = "text/html; charset=utf-8";
            response.Headers.Add("Server", "MyHttpServer");

            string custom404 = System.IO.Path.Combine(System.AppContext.BaseDirectory, "static", "404.html");
            byte[] buffer;

            if (System.IO.File.Exists(custom404))
            {
                buffer = await System.IO.File.ReadAllBytesAsync(custom404);
            }
            else
            {
                buffer = Encoding.UTF8.GetBytes("<h1>404 - Файл не найден</h1>");
            }

            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();

            Console.WriteLine("[NotFoundHandler] 404 Not Found");
        }
    }
}