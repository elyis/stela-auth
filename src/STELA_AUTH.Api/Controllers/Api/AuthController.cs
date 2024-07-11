using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STELA_AUTH.Core.Entities.Request;
using STELA_AUTH.Core.Entities.Response;
using STELA_AUTH.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace STELA_AUTH.Api.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(
            IAuthService authService,
            IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }


        [SwaggerOperation("Подать заявку на регистрацию")]
        [SwaggerResponse(200, "Успешно создан")]
        [SwaggerResponse(400, "Код не был отправлен на почту")]


        [HttpPost("apply-registration")]
        public async Task<IActionResult> ApplyForRegistrationAsync(SignUpBody body)
        {
            var result = await _authService.ApplyForRegistration(body.FirstName, body.LastName, body.Email, body.Password);
            return StatusCode((int)result);
        }

        [SwaggerOperation("Подтвердить регистрацию")]
        [SwaggerResponse(200, "Успешно создан", Type = typeof(OutputAccountCredentialsBody))]
        [SwaggerResponse(400, "Неверный метод верификации, ошибочный код или время жизни истекло")]
        [SwaggerResponse(404)]
        [SwaggerResponse(409, "Почта уже существует")]

        [HttpPost("signup")]
        public async Task<IActionResult> SignUpAsync(AccountVerificationBody body)
        {
            var result = await _authService.VerifyUnconfirmedAccount(body.Email, body.Code);
            if (result.StatusCode == HttpStatusCode.OK)
                return Ok(result.Body);

            return StatusCode((int)result.StatusCode);
        }


        [SwaggerOperation("Авторизация")]
        [SwaggerResponse(200, "Успешно", Type = typeof(OutputAccountCredentialsBody))]
        [SwaggerResponse(400, "Пароли не совпадают")]
        [SwaggerResponse(404, "Email не зарегистрирован")]

        [HttpPost("signin")]
        public async Task<IActionResult> SignInAsync(SignInBody body)
        {
            var result = await _authService.SignIn(body.Email, body.Password);
            if (result.StatusCode == HttpStatusCode.OK)
                return Ok(result.Body);

            return StatusCode((int)result.StatusCode);
        }

        [SwaggerOperation("Восстановление токена")]
        [SwaggerResponse(200, "Успешно создан", Type = typeof(OutputAccountCredentialsBody))]
        [SwaggerResponse(404, "Токен не используется")]

        [HttpPatch("restore-token")]
        public async Task<IActionResult> RestoreTokenAsync(TokenBody body)
        {
            var result = await _authService.RestoreToken(body.Value);
            if (result.StatusCode == HttpStatusCode.OK)
                return Ok(result.Body);

            return StatusCode((int)result.StatusCode);
        }

        [SwaggerOperation("Изменение пароля")]
        [SwaggerResponse(200, "Успешно создан")]
        [SwaggerResponse(400, "Пароли не совпадают")]

        [HttpPatch("change-password"), Authorize]
        public async Task<IActionResult> ChangePassword(
            UpdatePasswordBody body,
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _authService.ChangePassword(tokenPayload.AccountId, body.Password, body.NewPassword);
            return StatusCode((int)result);
        }
    }
}