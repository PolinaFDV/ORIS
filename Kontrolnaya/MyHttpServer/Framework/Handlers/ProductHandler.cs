using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MyHttpServer.Framework.Core;
using MyHttpServer.Models;

namespace MyHttpServer.Handlers
{
    public class ProductHandler
    {
        private static List<Product> Items = new List<Product>
        {
            new Product { Id = 1, Name = "Ноутбук", Price = 50000, Status = "available" },
            new Product { Id = 2, Name = "Мышь", Price = 1500, Status = "available" }
        };

        [HttpGet("/products")]
        public async Task HandleList(HttpListenerContext context)
        {
            string minPriceStr = context.Request.QueryString["minPrice"];
            int minPrice = 0;
            if (int.TryParse(minPriceStr, out int mp)) minPrice = mp;

            var filtered = Items.Where(p => p.Price >= minPrice).ToList();
            int totalSum = filtered.Sum(p => p.Price);

            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'></head><body>");
            sb.Append("<h1>Товары</h1>");
            sb.Append("<form method='get' action='/products'>");
            sb.Append("Мин. цена: <input type='number' name='minPrice' value='" + minPrice + "'> <button>Фильтр</button>");
            sb.Append("</form>");
            sb.Append("<p><a href='/products/create'>Добавить товар</a></p>");
            sb.Append("<table border='1'><tr><th>Id</th><th>Название</th><th>Цена</th><th>Статус</th></tr>");
            foreach (var p in filtered)
            {
                sb.Append($"<tr><td>{p.Id}</td><td>{WebUtility.HtmlEncode(p.Name)}</td><td>{p.Price}</td><td>{p.Status}</td></tr>");
            }
            sb.Append("</table>");
            sb.Append($"<p>Сумма: {totalSum}</p>");
            sb.Append("</body></html>");

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
        }

        [HttpPost("/products/create")]
        public async Task HandleCreate(HttpListenerContext context)
        {
            using StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            string formData = await reader.ReadToEndAsync();

            string name = "";
            string priceStr = "";

            string[] fields = formData.Split('&');
            foreach (string field in fields)
            {
                string[] parts = field.Split('=', 2);
                if (parts.Length != 2) continue;
                
                string key = WebUtility.UrlDecode(parts[0]);
                string val = WebUtility.UrlDecode(parts[1]);

                if (key == "name") name = val;
                if (key == "price") priceStr = val;
            }

            if (string.IsNullOrEmpty(name) || name.Length < 2 || !int.TryParse(priceStr, out int price))
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
                return;
            }

            Items.Add(new Product { Id = Items.Count + 1, Name = name, Price = price, Status = "available" });

            context.Response.StatusCode = 303;
            context.Response.RedirectLocation = "/products";
            context.Response.OutputStream.Close();
        }

        [HttpPut("/products/update")]
        public async Task HandleUpdate(HttpListenerContext context)
        {
            string idStr = context.Request.QueryString["id"];
            if (!int.TryParse(idStr, out int id))
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
                return;
            }

            var item = Items.FirstOrDefault(p => p.Id == id);
            if (item == null)
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }

            using StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            string formData = await reader.ReadToEndAsync();

            string name = item.Name;
            string priceStr = item.Price.ToString();
            string status = item.Status;

            string[] fields = formData.Split('&');
            foreach (string field in fields)
            {
                string[] parts = field.Split('=', 2);
                if (parts.Length != 2) continue;
                
                string key = WebUtility.UrlDecode(parts[0]);
                string val = WebUtility.UrlDecode(parts[1]);

                if (key == "name") name = val;
                if (key == "price") priceStr = val;
                if (key == "status") status = val;
            }

            if (int.TryParse(priceStr, out int price))
            {
                item.Name = name;
                item.Price = price;
                item.Status = status;
            }

            context.Response.StatusCode = 204;
            context.Response.Close();
        }
        
                [HttpGet("/products/details")]
        public async Task HandleDetails(HttpListenerContext context)
        {
            string idStr = context.Request.QueryString["id"];
            if (!int.TryParse(idStr, out int id))
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "text/plain; charset=utf-8";
                byte[] err = Encoding.UTF8.GetBytes("Неверный id");
                context.Response.ContentLength64 = err.Length;
                await context.Response.OutputStream.WriteAsync(err, 0, err.Length);
                context.Response.Close();
                return;
            }

            var p = Items.FirstOrDefault(x => x.Id == id);
            if (p == null)
            {
                context.Response.StatusCode = 404;
                context.Response.ContentType = "text/plain; charset=utf-8";
                byte[] err = Encoding.UTF8.GetBytes("Товар не найден");
                context.Response.ContentLength64 = err.Length;
                await context.Response.OutputStream.WriteAsync(err, 0, err.Length);
                context.Response.Close();
                return;
            }

            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'></head><body>");
            sb.Append($"<h1>{WebUtility.HtmlEncode(p.Name)}</h1>");
            sb.Append($"<p>Id: {p.Id}</p>");
            sb.Append($"<p>Цена: {p.Price}</p>");
            sb.Append($"<p>Статус: {WebUtility.HtmlEncode(p.Status)}</p>");
            sb.Append("<p><a href='/products'>Назад</a></p>");
            sb.Append("</body></html>");

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
        }
        
        [HttpGet("/products/create")]
        public async Task HandleCreatePage(HttpListenerContext context)
        {
            string html = @"<!DOCTYPE html>
            <html>
            <head><meta charset='utf-8'></head>
            <body>
                <h1>Добавить товар</h1>
                <form method='post' action='/products/create'>
                    Название: <input type='text' name='name' required><br><br>
                    Цена: <input type='number' name='price' required><br><br>
                    <button type='submit'>Создать</button>
                    <a href='/products'>Отмена</a>
                </form>
            </body>
            </html>";
                        
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
        }
        
                [HttpGet("/products/update")]
        public async Task HandleUpdatePage(HttpListenerContext context)
        {
            string idStr = context.Request.QueryString["id"];
            if (!int.TryParse(idStr, out int id)) id = 1;

            var p = Items.FirstOrDefault(x => x.Id == id);
            if (p == null)
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }

            string html = $@"
            <html>
            <head><meta charset='utf-8'></head>
            <body>
                <h1>Редактировать: {WebUtility.HtmlEncode(p.Name)}</h1>
                
                <form id='updateForm'>
                    Название: <input type='text' id='name' value='{WebUtility.HtmlEncode(p.Name)}' required><br><br>
                    Цена: <input type='number' id='price' value='{p.Price}' required><br><br>
                    Статус: 
                    <select id='status'>
                        <option value='available' {(p.Status == "available" ? "selected" : "")}>available</option>
                        <option value='unavailable' {(p.Status == "unavailable" ? "selected" : "")}>unavailable</option>
                    </select><br><br>
                    
                    <button type='button' onclick='sendUpdate({p.Id})'>Сохранить (PUT)</button>
                    <a href='/products'>Отмена</a>
                </form>

                <script>
                    function sendUpdate(id) {{
                        var name = document.getElementById('name').value;
                        var price = document.getElementById('price').value;
                        var status = document.getElementById('status').value;
                        
                        var formData = 'name=' + encodeURIComponent(name) + '&price=' + encodeURIComponent(price) + '&status=' + encodeURIComponent(status);

                        fetch('/products/update?id=' + id, {{
                            method: 'PUT',
                            headers: {{ 'Content-Type': 'application/x-www-form-urlencoded' }},
                            body: formData
                        }})
                        .then(response => {{
                            if (response.status === 204) {{
                                alert('Товар успешно обновлен!');
                                window.location.href = '/products';
                            }} else {{
                                alert('Ошибка при обновлении. Код: ' + response.status);
                            }}
                        }})
                        .catch(err => alert('Ошибка сети: ' + err));
                    }}
                </script>
            </body>
            </html>";

            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
        }
    }
}