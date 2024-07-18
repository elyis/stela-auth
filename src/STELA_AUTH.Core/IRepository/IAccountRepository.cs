using STELA_AUTH.Core.Entities.Models;
using STELA_AUTH.Core.Enums;

namespace STELA_AUTH.Core.IRepository
{
    public interface IAccountRepository
    {
        Task<Account?> AddAsync(string firstName, string lastName, string email, string passwordHash, string role);
        Task<Account?> GetById(Guid id);
        Task<Account?> GetByEmail(string email);
        Task<Account?> ChangePassword(Guid id, string password, string newPassword);
        Task<string?> UpdateTokenAsync(string refreshToken, Guid accountId, TimeSpan? duration = null);
        Task<Account?> GetByTokenAsync(string refreshTokenHash);
        Task<Account?> UpdateConfirmationCode(Guid id, string code);
        Task<Account?> VerifyConfirmationCode(Guid id, string email, string code);
        Task<IEnumerable<Account>> GetAllAccounts(int count, int offset, bool isOrderByAscending = true);
        Task<Account?> Update(Guid id, string? firstName, string? lastName, AccountRole? role);
        Task<int> GetTotalAccounts();
        Task<Account?> Update(Guid id, string? firstName, string? lastName);
        Task<Account?> UpdateImage(Guid accountId, string filename);
    }
}