using System.Text.Json;
using MyHttpServer;
using MyHttpServer.Framework.Core;

Settings settings;
try
{
    string json = File.ReadAllText("settings.json");
    settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
}
catch
{
    Console.WriteLine("Не удалось прочитать settings.json, использую настройки по умолчанию.");
    settings = new Settings { Prefixes = new[] { "http://127.0.0.1:8888/" } };
}

HttpServer httpServer = new HttpServer(settings);
httpServer.Start();

Console.WriteLine("Нажмите Enter для остановки сервера...");
Console.ReadLine();
httpServer.Stop();