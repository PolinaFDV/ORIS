using System.Text.Json;
using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Results;
using MyHttpServer.Models;
using MiniHttpServer.ORM;

namespace MyHttpServer.Controllers;

[HttpController("user")]
public class UserController : BaseController
{
    [Get("profile")]
    public IHttpResult Profile()
    {
        var user = new { Name = "Alex", Age = 20, Email = "alex@example.com", IsActive = true, Roles = new[] { "Admin", "User" } };
        return Page("User/Profile.html", user);
    }

    [Get("list")]
    public async Task<IHttpResult> List()
    {
        string settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsPath));
        
        IOrmContext orm = new OrmContext(settings!.ConnectionString);
        IReadOnlyList<User> users = await orm.GetAllAsync<User>("Users");
        
        return Page("User/List.html", new { Users = users });
    }

    [Get("products")]
    public async Task<IHttpResult> Products()
    {
        string settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsPath));
        
        IOrmContext orm = new OrmContext(settings!.ConnectionString);
        IReadOnlyList<Product> products = await orm.GetAllAsync<Product>("Products");
        
        return Json(products);
    }
}