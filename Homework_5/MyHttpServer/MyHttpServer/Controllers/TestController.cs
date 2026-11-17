using MyHttpServer.Framework;
using MyHttpServer.Framework.Core;

namespace MyHttpServer.Controllers;

[HttpController("test")]
public class TestController : ControllerBase
{
    [Get("index")]
    public string Index() => "Hello from controller!";

    [Get("hello")]
    public string Hello() => "Hello World!";

    [Post("submit")]
    public string Submit() => "POST request processed";

    [Get("details")]
    public string Details([FromQuery] int id, [FromQuery] string name)
        => $"ID: {id}, Name: {name}";
}