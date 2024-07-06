using System.Net;
using STELA_AUTH.Core.Entities.Request;
using STELA_AUTH.Core.Entities.Response;

namespace STELA_AUTH.Core.IService
{
    public interface IAccountService
    {
        Task<ServiceResponse<ProfileBody>> GetById(Guid accountId);
        Task<HttpStatusCode> PatchAccountCredentials(Guid accountId, PatchAccountCredentialsBody body);
        Task<HttpStatusCode> VerifyConfirmationCode(Guid accountId, string email, string code);
    }
}