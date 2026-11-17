using System.Threading.Tasks;

namespace MyHttpServer.Services;

public interface IEmailService
{
    Task<bool> SendAsync(string to, string subject, string body);
}