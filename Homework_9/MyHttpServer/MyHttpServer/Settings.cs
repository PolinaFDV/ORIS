namespace MyHttpServer;

public class Settings
{
    public string[] Prefixes { get; set; } = [];
    public string ConnectionString { get; set; } = string.Empty;
    public EmailSettings Email { get; set; } = new();
}

public class EmailSettings
{
    public string Host { get; set; } = "smtp.yandex.ru";
    public int Port { get; set; } = 587;
    public string SenderEmail { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string Password { get; set; } = "";
}