using System.Net;
using STELA_AUTH.Core.Entities.Response;
using STELA_AUTH.Core.Enums;
using STELA_AUTH.Core.IRepository;
using STELA_AUTH.Core.IService;
using STELA_AUTH.Shared.Entities;
using STELA_AUTH.Shared.Provider;

namespace STELA_AUTH.App.Service
{
    public class AuthService : IAuthService
    {
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly Hmac512Provider _passwordHashProvider;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnconfirmedAccountRepository _unconfirmedAccountRepository;

        public AuthService(
            IJwtService jwtService,
            IEmailService emailService,
            Hmac512Provider passwordHashProvider,
            IAccountRepository accountRepository,
            IUnconfirmedAccountRepository unconfirmedAccountRepository)
        {
            _jwtService = jwtService;
            _emailService = emailService;
            _passwordHashProvider = passwordHashProvider;
            _accountRepository = accountRepository;
            _unconfirmedAccountRepository = unconfirmedAccountRepository;
        }

        public async Task<HttpStatusCode> ApplyForRegistration(
            string firstName,
            string lastName,
            string email,
            string password)
        {
            var account = await _accountRepository.GetByEmail(email);
            if (account != null)
                return HttpStatusCode.Conflict;

            var code = CodeGeneratorService.Generate();
            await _unconfirmedAccountRepository.Remove(email);
            await _unconfirmedAccountRepository.AddAsync(firstName, lastName, email, password, code);

            var serviceResponseCode = await _emailService.SendMessage(email, "Confirm account", $"Confirmation code: {code}");
            return serviceResponseCode;
        }

        public async Task<ServiceResponse<OutputAccountCredentialsBody>> RestoreToken(string refreshToken)
        {
            var user = await _accountRepository.GetByTokenAsync(refreshToken);
            if (user != null)
            {
                var serviceResponse = await UpdateToken(user.RoleName, user.Id);
                return serviceResponse;
            }

            return new ServiceResponse<OutputAccountCredentialsBody>
            {
                IsSuccess = false,
                Body = null,
                StatusCode = HttpStatusCode.NotFound
            };
        }

        public async Task<ServiceResponse<OutputAccountCredentialsBody>> SignIn(string email, string password)
        {
            var user = await _accountRepository.GetByEmail(email);
            if (user == null)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    IsSuccess = false,
                    Body = null,
                    StatusCode = HttpStatusCode.NotFound
                };

            var inputPasswordHash = _passwordHashProvider.Compute(password);
            if (user.PasswordHash != inputPasswordHash)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    IsSuccess = false,
                    Body = null,
                    StatusCode = HttpStatusCode.BadRequest
                };

            var serviceResponse = await UpdateToken(user.RoleName, user.Id);
            return serviceResponse;
        }

        public async Task<ServiceResponse<OutputAccountCredentialsBody>> UpdateToken(string rolename, Guid userId)
        {
            var tokenInfo = new TokenPayload
            {
                Role = rolename,
                AccountId = userId
            };

            var tokenPair = _jwtService.GenerateDefaultTokenPair(tokenInfo);
            tokenPair.RefreshToken = await _accountRepository.UpdateTokenAsync(tokenPair.RefreshToken, tokenInfo.AccountId);
            tokenPair.Role = Enum.Parse<AccountRole>(rolename);
            return new ServiceResponse<OutputAccountCredentialsBody>
            {
                IsSuccess = true,
                Body = tokenPair,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ServiceResponse<OutputAccountCredentialsBody>> VerifyUnconfirmedAccount(string email, string code)
        {
            var account = await _unconfirmedAccountRepository.GetByEmail(email);
            if (account == null || account.ConfirmationCode != code || account.ConfirmationCodeValidBefore < DateTime.UtcNow)
            {
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    IsSuccess = false,
                    Body = null,
                    StatusCode = account == null ? HttpStatusCode.NotFound : HttpStatusCode.BadRequest
                };
            }

            var rolename = AccountRole.User.ToString();
            var result = await _accountRepository.AddAsync(account.FirstName, account.LastName, account.Email, account.PasswordHash, rolename);
            return await UpdateToken(rolename, result.Id);
        }

        public async Task<HttpStatusCode> ChangePassword(Guid accountId, string password, string newPassword)
        {
            var result = await _accountRepository.ChangePassword(accountId, password, newPassword);
            return result == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK;
        }
    }
}