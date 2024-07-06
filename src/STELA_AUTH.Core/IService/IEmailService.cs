using System.Net;

namespace STELA_AUTH.Core.IService
{
    public interface IEmailService
    {
        Task<HttpStatusCode> SendMessage(string recipientEmail, string subject, string message);
    }
}