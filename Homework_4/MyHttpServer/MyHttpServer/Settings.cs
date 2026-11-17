namespace MyHttpServer;

public class Settings
{
    public static Settings Instance { get; } = new Settings();

    public string[] Prefixes { get; set; } = new[] { "http://127.0.0.1:8888/" };
}