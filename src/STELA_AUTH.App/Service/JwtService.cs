using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using STELA_AUTH.Core.Entities.Response;
using STELA_AUTH.Core.IService;
using STELA_AUTH.Shared.Entities;

namespace STELA_AUTH.App.Service
{
    public class JwtService : IJwtService
    {
        private readonly SigningCredentials _signingCredentials;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtService(string key, string issuer, string audience)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            _issuer = issuer;
            _audience = audience;
        }

        private string GenerateAccessToken(Dictionary<string, string> claims, TimeSpan timeSpan)
        {
            var tokenClaims = claims.Select(claim => new Claim(claim.Key, claim.Value));

            var token = new JwtSecurityToken(
                claims: tokenClaims,
                expires: DateTime.UtcNow.Add(timeSpan),
                signingCredentials: _signingCredentials,
                issuer: _issuer,
                audience: _audience
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken() => Guid.NewGuid().ToString();

        public OutputAccountCredentialsBody GenerateDefaultTokenPair(TokenPayload tokenPayload)
        {
            var claims = new Dictionary<string, string>{
                { "AccountId", tokenPayload.AccountId.ToString() },
                { ClaimTypes.Role, tokenPayload.Role},
            };
            var timeSpan = new TimeSpan(2, 0, 0, 0);
            return GenerateTokenPair(claims, timeSpan);
        }

        private OutputAccountCredentialsBody GenerateTokenPair(Dictionary<string, string> claims, TimeSpan timeSpan) =>
            new(
                    GenerateAccessToken(claims, timeSpan),
                    GenerateRefreshToken()
                );

        private List<Claim> GetClaims(string token) =>
            new JwtSecurityTokenHandler()
                .ReadJwtToken(token.Replace("Bearer ", ""))
                .Claims
                .ToList();

        public TokenPayload GetTokenPayload(string token)
        {
            var claims = GetClaims(token);
            return new TokenPayload
            {
                Role = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value,
                AccountId = Guid.Parse(claims.FirstOrDefault(claim => claim.Type == "AccountId")?.Value),
            };
        }
    }
}