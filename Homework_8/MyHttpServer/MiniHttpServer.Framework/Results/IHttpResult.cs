using System.Net;
using System.Threading;

namespace MyHttpServer.Framework.Results;

public interface IHttpResult
{
    Task ExecuteAsync(
        HttpListenerResponse response,
        CancellationToken cancellationToken = default);
}