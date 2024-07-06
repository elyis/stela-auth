using System.Net;
using STELA_AUTH.Core.Entities.Response;

namespace STELA_AUTH.Core.IService
{
    public interface IAuthService
    {
        Task<HttpStatusCode> ApplyForRegistration(string firstName, string lastName, string email, string password);
        Task<ServiceResponse<OutputAccountCredentialsBody>> SignIn(string email, string password);
        Task<ServiceResponse<OutputAccountCredentialsBody>> RestoreToken(string refreshToken);
        Task<ServiceResponse<OutputAccountCredentialsBody>> VerifyUnconfirmedAccount(string email, string code);
        Task<ServiceResponse<OutputAccountCredentialsBody>> UpdateToken(string rolename, Guid userId);
        Task<HttpStatusCode> ChangePassword(Guid accountId, string password, string newPassword);
    }
}