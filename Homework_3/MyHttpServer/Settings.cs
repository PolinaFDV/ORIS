using System;
using System.IO;
using System.Text.Json;

namespace MyHttpServer
{
    public class ServerSettings
    {
        private static readonly Lazy<ServerSettings> _instance = 
            new Lazy<ServerSettings>(() => new ServerSettings());

        public static ServerSettings Instance => _instance.Value;

        public string[] Prefixes { get; set; } = Array.Empty<string>();

        private ServerSettings()
        {
            Load();
        }

        private void Load()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "settings.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var settings = JsonSerializer.Deserialize<ServerSettings>(json);
                if (settings != null && settings.Prefixes.Length > 0)
                {
                    Prefixes = settings.Prefixes;
                }
            }
            else
            {
                Prefixes = new[] { "http://127.0.0.1:8888/" };
            }
        }
    }
}