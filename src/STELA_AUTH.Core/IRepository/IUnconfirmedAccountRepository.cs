using STELA_AUTH.Core.Entities.Models;

namespace STELA_AUTH.Core.IRepository
{
    public interface IUnconfirmedAccountRepository
    {
        Task<UnconfirmedAccount?> GetByEmail(string email);
        Task<UnconfirmedAccount?> AddAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            string confirmationCode);
        Task<bool> Remove(string email);
    }
}