using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Results;
using System.Text.Json;
using MyHttpServer.Models;
using MiniHttpServer.ORM;

namespace MyHttpServer.Controllers;

[HttpController("user")]
public class UserController : BaseController
{
    [Get("profile")]
    [Authorize]
    public IHttpResult Profile()
    {
        return Html("<h1>Личный кабинет</h1>");
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
}