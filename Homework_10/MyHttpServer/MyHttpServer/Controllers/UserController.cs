using System.Text.Json;
using MyHttpServer.Framework;
using MyHttpServer.Framework.Core;
using MyHttpServer.Models;
using MiniHttpServer.ORM;
using MiniTemplateEngine;

namespace MyHttpServer.Controllers;

[HttpController("user")]
public class UserController : ControllerBase
{
    [Get("list")]
    public async Task<string> List()
    {
        string settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsPath));
        
        IOrmContext orm = new OrmContext(settings!.ConnectionString);
        IReadOnlyList<User> users = await orm.GetAllAsync<User>("Users");

        var renderer = new HtmlTemplateRenderer();
        string templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "User", "List.html");
        return renderer.RenderFromFile(templatePath, new { Users = users });
    }
}