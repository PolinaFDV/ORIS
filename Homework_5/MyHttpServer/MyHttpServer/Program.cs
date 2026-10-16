using MyHttpServer.Framework.Core;

HttpServer httpServer = new HttpServer();

httpServer.Start();

Console.ReadLine();

httpServer.Stop();