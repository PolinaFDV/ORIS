using System.Text.Json;
using MiniHttpServer.ORM;
using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Results;
using MyHttpServer.Models;

namespace MyHttpServer.Controllers;

[HttpController("home")]
public class HomeController : BaseController
{
    private static readonly Lazy<IOrmContext> LazyOrm = new(() =>
    {
        string settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsPath));
        return new OrmContext(settings!.ConnectionString);
    });

    private IOrmContext Orm => LazyOrm.Value;

    [Get("index")]
    public async Task<IHttpResult> Index()
    {
        var cards = await Orm.GetAllAsync<Card>("cards");
        return Page("index.html", new { Cards = cards });
    }

    [Get("card")]
    public async Task<IHttpResult> CardDetail([FromQuery] int id)
    {
        var card = await Orm.GetByIdAsync<Card>("cards", id);
        if (card == null) return Html("<h1>Карточка не найдена</h1>", 404);

        var expansion = await Orm.GetByIdAsync<Expansion>("expansions", card.ExpansionId);

        return Page("card.html", new { Card = card, Expansion = expansion });
    }

    [Get("expansions")]
    public async Task<IHttpResult> Expansions()
    {
        var expansions = await Orm.GetAllAsync<Expansion>("expansions");
        return Page("expansions.html", new { Expansions = expansions });
    }

    [Get("faq")]
    public async Task<IHttpResult> Faq()
    {
        var faq = await Orm.GetAllAsync<FaqItem>("faq");
        return Page("faq.html", new { Faq = faq });
    }

    [Get("rules")]
    public IHttpResult Rules()
    {
        return Page("rules.html", new { });
    }

    [Post("feedback")]
    public async Task<IHttpResult> SaveFeedback(
        [FromForm] string name,
        [FromForm] string email,
        [FromForm] string message)
    {
        string safeName = SanitizeInput(name);
        string safeMessage = SanitizeInput(message);

        if (safeName.Length > 50 || safeMessage.Length > 500)
        {
            return Json(new { success = false, message = "Слишком длинное сообщение." }, 400);
        }

        var fb = new Feedback
        {
            Name = safeName,
            Email = email, 
            Message = safeMessage
        };

        await Orm.InsertAsync("feedback", fb);
        return Json(new { success = true, message = "Спасибо! Сообщение отправлено." });
    }

    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        string result = System.Text.RegularExpressions.Regex.Replace(
            input, @"<\s*script[^>]*>.*?<\s*/\s*script\s*>", "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
                      | System.Text.RegularExpressions.RegexOptions.Singleline);
        result = System.Text.RegularExpressions.Regex.Replace(
            result, @"<\s*script[^>]*/?>", "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        result = System.Text.RegularExpressions.Regex.Replace(
            result, @"\son\w+\s*=\s*(""[^""]*""|'[^']*')", "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return result;
    }
}