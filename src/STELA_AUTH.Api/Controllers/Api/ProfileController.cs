using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STELA_AUTH.Core.Entities.Request;
using STELA_AUTH.Core.Entities.Response;
using STELA_AUTH.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace STELA_AUTH.Api.Controllers.Api
{
    [ApiController]
    [Route("api")]
    public class ProfileController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IAccountService _accountService;

        public ProfileController(
            IJwtService jwtService,
            IEmailService emailService,
            IAccountService accountService)
        {
            _jwtService = jwtService;
            _emailService = emailService;
            _accountService = accountService;
        }


        [HttpGet("me"), Authorize]
        [SwaggerOperation("Получить профиль")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(ProfileBody))]
        public async Task<IActionResult> GetProfileAsync(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _accountService.GetById(tokenPayload.AccountId);

            if (result.StatusCode == HttpStatusCode.OK)
                return Ok(result.Body);

            return StatusCode((int)result.StatusCode);
        }

        [HttpPatch("me"), Authorize]
        [SwaggerOperation("Обновить данные пользователя")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(404)]

        public async Task<IActionResult> UpdateAccount(
            PatchAccountCredentialsBody body,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _accountService.PatchAccountCredentials(tokenPayload.AccountId, body);
            return StatusCode((int)result);
        }

        // [HttpPatch("me/email"), Authorize]
        // [SwaggerOperation("Обновить почту пользователя")]
        // [SwaggerResponse(200)]
        // [SwaggerResponse(400)]
        // [SwaggerResponse(404)]

        // public async Task<IActionResult> UpdateEmail(
        //     PatchEmailBody body,
        //     [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        // {
        //     var tokenPayload = _jwtService.GetTokenPayload(token);

        //     var code = CodeGeneratorService.Generate();
        //     var isMessageSent = await _emailService.SendMessage(body.Email, "Confirm email", code);
        //     if (!isMessageSent)
        //         return BadRequest("message was not delivered");

        //     var result = await _accountRepository.UpdateConfirmationCode(tokenPayload.UserId, code);
        //     return result == null ? NotFound() : Ok();
        // }

        [HttpPatch("me/verify"), Authorize]
        [SwaggerOperation("Верифицировать почту")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]

        public async Task<IActionResult> VerifyPhoneOrEmail(
            AccountVerificationBody body,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _accountService.VerifyConfirmationCode(tokenPayload.AccountId, body.Email, body.Code);
            return StatusCode((int)result);
        }
    }
}