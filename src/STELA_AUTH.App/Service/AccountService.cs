using System.Net;
using STELA_AUTH.Core.Entities.Request;
using STELA_AUTH.Core.Entities.Response;
using STELA_AUTH.Core.IRepository;
using STELA_AUTH.Core.IService;

namespace STELA_AUTH.App.Service
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<ServiceResponse<ProfileBody>> GetById(Guid accountId)
        {
            var account = await _accountRepository.GetById(accountId);
            bool isFound = account != null;

            return new ServiceResponse<ProfileBody>
            {
                StatusCode = isFound ? HttpStatusCode.OK : HttpStatusCode.NotFound,
                Body = !isFound ? null : account.ToProfileBody(),
                IsSuccess = isFound
            };
        }

        public async Task<HttpStatusCode> PatchAccountCredentials(Guid accountId, PatchAccountCredentialsBody body)
        {
            var result = await _accountRepository.Update(accountId, body.FirstName, body.LastName, body.Role);
            return result == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK;
        }

        public async Task<HttpStatusCode> VerifyConfirmationCode(Guid accountId, string email, string code)
        {
            var result = await _accountRepository.VerifyConfirmationCode(accountId, email, code);
            return result == null ? HttpStatusCode.NotFound : HttpStatusCode.OK;
        }
    }
}