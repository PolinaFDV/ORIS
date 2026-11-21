using MyHttpServer.Framework.Core;

namespace MyHttpServer;

public static class Program
{
    public static void Main(string[] args)
    {
        HttpServer server = new HttpServer();
        server.Start();

        Thread.Sleep(Timeout.Infinite);
    }
}