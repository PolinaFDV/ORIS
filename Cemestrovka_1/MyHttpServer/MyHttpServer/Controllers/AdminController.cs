using System.Text;
using System.Text.Json;
using MiniHttpServer.ORM;
using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Core;
using MyHttpServer.Framework.Results;
using MyHttpServer.Models;

namespace MyHttpServer.Controllers;

[HttpController("admin")]
[Authorize]
public class AdminController : BaseController
{
    private const int MaxImageSizeBytes = 5 * 1024 * 1024; 

    private static readonly Lazy<IOrmContext> LazyOrm = new(() =>
    {
        string settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsPath));
        return new OrmContext(settings!.ConnectionString);
    });

    private IOrmContext Orm => LazyOrm.Value;

    [Get("cards")]
    public async Task<IHttpResult> GetAllCards()
    {
        var cards = await Orm.GetAllAsync<Card>("cards");
        return Json(cards);
    }

    [Post("upload-image")]
    public async Task<IHttpResult> UploadImage([FromForm] string base64Data)
    {
        if (string.IsNullOrEmpty(base64Data))
            return Json(new { success = false, message = "Нет данных" });

        try
        {
            string pureBase64 = base64Data;
            if (base64Data.Contains(","))
                pureBase64 = base64Data.Split(',')[1];

            if (pureBase64.Length > MaxImageSizeBytes * 4 / 3)
                return Json(new { success = false, message = "Файл слишком большой (макс. 5 МБ)." });

            byte[] imageBytes = Convert.FromBase64String(pureBase64);

            if (!IsValidImage(imageBytes))
                return Json(new { success = false, message = "Недопустимый формат изображения." });

            string fileName = Guid.NewGuid().ToString("N") + (IsPng(imageBytes) ? ".png" : ".jpg");

            string uploadsDir = Path.Combine(AppContext.BaseDirectory, "static", "uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            string filePath = Path.Combine(uploadsDir, fileName);
            await File.WriteAllBytesAsync(filePath, imageBytes);

            return Json(new { success = true, url = "uploads/" + fileName });
        }
        catch
        {
            return Json(new { success = false, message = "Не удалось загрузить изображение." });
        }
    }

    [Post("create")]
    public async Task<IHttpResult> CreateCard(
        [FromForm] string title,
        [FromForm] string type,
        [FromForm] string description,
        [FromForm] string imageUrl,
        [FromForm] int expansionId = 1)
    {
        var card = new Card
        {
            Title = title,
            Type = type,
            Description = description,
            ImageUrl = imageUrl,
            ExpansionId = expansionId
        };
        await Orm.InsertAsync("cards", card);
        return Json(new { success = true });
    }

    [Post("update")]
    public async Task<IHttpResult> UpdateCard(
        [FromForm] int id,
        [FromForm] string title,
        [FromForm] string type,
        [FromForm] string description,
        [FromForm] string imageUrl,
        [FromForm] int expansionId = 1)
    {
        var card = new Card
        {
            Id = id,
            Title = title,
            Type = type,
            Description = description,
            ImageUrl = imageUrl,
            ExpansionId = expansionId
        };
        await Orm.UpdateAsync("cards", id, card);
        return Json(new { success = true });
    }

    [Post("delete")]
    public async Task<IHttpResult> DeleteCard([FromForm] int id)
    {
        await Orm.DeleteAsync("cards", id);
        return Json(new { success = true });
    }

    private static bool IsPng(byte[] bytes) =>
        bytes.Length >= 8 &&
        bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
        bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;

    private static bool IsJpeg(byte[] bytes) =>
        bytes.Length >= 3 &&
        bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;

    private static bool IsValidImage(byte[] bytes) => IsPng(bytes) || IsJpeg(bytes);
}