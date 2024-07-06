using STELA_AUTH.Core.Entities.Response;
using STELA_AUTH.Shared.Entities;

namespace STELA_AUTH.Core.IService
{
    public interface IJwtService
    {
        OutputAccountCredentialsBody GenerateDefaultTokenPair(TokenPayload tokenPayload);
        TokenPayload GetTokenPayload(string token);
    }
}