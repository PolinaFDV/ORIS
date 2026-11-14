using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MyHttpServer.Framework.Core;

namespace MyHttpServer.Handlers
{
    public class LoginHandler
    {
        [HttpPost("/login")]
        public async Task HandleLogin(HttpListenerContext context)
        {
            using StreamReader reader = new StreamReader(
                context.Request.InputStream,
                context.Request.ContentEncoding
            );

            string formData = await reader.ReadToEndAsync();

            LoginData loginData = new LoginData();

            string[] fields = formData.Split('&');

            foreach (string field in fields)
            {
                string[] parts = field.Split('=', 2);

                if (parts.Length != 2)
                {
                    continue;
                }

                string name = WebUtility.UrlDecode(parts[0]);
                string value = WebUtility.UrlDecode(parts[1]);

                if (name == "login")
                {
                    loginData.Login = value;
                }
                else if (name == "password")
                {
                    loginData.Password = value;
                }
            }

            Console.WriteLine("Получены данные формы:");
            Console.WriteLine($"Логин: {loginData.Login}");
            Console.WriteLine($"Пароль: {loginData.Password}");

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain; charset=utf-8";
            
            byte[] responseBytes = Encoding.UTF8.GetBytes(
                "Данные получены. Проверьте консоль сервера."
            );
            
            context.Response.ContentLength64 = responseBytes.Length;
            
            await context.Response.OutputStream.WriteAsync(
                responseBytes, 0, responseBytes.Length
            );

            context.Response.Close();
        }
    }
}