using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Results;

namespace MyHttpServer.Controllers;

[HttpController("user")]
public class UserController : BaseController
{
    [Get("profile")]
    public IHttpResult Profile()
    {
        var user = new
        {
            Name = "Alex",
            Age = 20,
            Email = "alex@example.com",
            IsActive = true,
            Roles = new[] { "Admin", "User", "Guest" }
        };

        return Page("User/Profile.html", user);
    }
    
    [Get("list")]
    public IHttpResult List()
    {
        var users = new List<object>
        {
            new { Name = "Alex", IsActive = true },
            new { Name = "John", IsActive = false },
            new { Name = "Anna", IsActive = true }
        };
    
        return Page("User/List.html", new { Users = users });
    }
}