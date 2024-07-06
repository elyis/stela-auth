using Microsoft.EntityFrameworkCore;
using STELA_AUTH.Core.Entities.Models;
using STELA_AUTH.Core.IRepository;
using STELA_AUTH.Infrastructure.Data;
using STELA_AUTH.Shared.Provider;

namespace STELA_AUTH.Infrastructure.Repository
{
    public class UnconfirmedAccountRepository : IUnconfirmedAccountRepository
    {
        private readonly AuthDbContext _context;
        private readonly Hmac512Provider _passwordHasher;

        public UnconfirmedAccountRepository(AuthDbContext context, Hmac512Provider passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<UnconfirmedAccount?> AddAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            string confirmationCode)
        {
            var account = await GetByEmail(email);
            if (account != null)
                return null;

            account = new UnconfirmedAccount
            {
                Email = email,
                ConfirmationCode = confirmationCode,
                ConfirmationCodeValidBefore = DateTime.UtcNow.AddMinutes(5),
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = _passwordHasher.Compute(password)
            };

            account = (await _context.UnconfirmedAccounts.AddAsync(account))?.Entity;
            await _context.SaveChangesAsync();

            return account;
        }

        public async Task<UnconfirmedAccount?> GetByEmail(string email) =>
            await _context.UnconfirmedAccounts.FirstOrDefaultAsync(e => e.Email == email);

        public async Task<bool> Remove(string email)
        {
            var account = await GetByEmail(email);
            if (account == null)
                return true;

            _context.UnconfirmedAccounts.Remove(account);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}