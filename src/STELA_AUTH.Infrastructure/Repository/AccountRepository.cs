using Microsoft.EntityFrameworkCore;
using STELA_AUTH.Core.Entities.Models;
using STELA_AUTH.Core.Enums;
using STELA_AUTH.Core.IRepository;
using STELA_AUTH.Infrastructure.Data;
using STELA_AUTH.Shared.Provider;

namespace STELA_AUTH.Infrastructure.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AuthDbContext _context;
        private readonly Hmac512Provider _passwordHasher;
        public AccountRepository(AuthDbContext context, Hmac512Provider passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Account?> GetById(Guid id)
            => await _context.Accounts
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<Account?> GetByEmail(string email)
            => await _context.Accounts
                .FirstOrDefaultAsync(e => e.Email == email);

        public async Task<Account?> GetByTokenAsync(string refreshTokenHash)
            => await _context.Accounts
            .FirstOrDefaultAsync(e => e.Token == refreshTokenHash);

        public async Task<Account?> UpdateImage(Guid userId, string filename)
        {
            var user = await GetById(userId);
            if (user == null)
                return null;

            user.Image = filename;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<string?> UpdateTokenAsync(string refreshToken, Guid userId, TimeSpan? duration = null)
        {
            var user = await GetById(userId);
            if (user == null)
                return null;

            if (duration == null)
                duration = TimeSpan.FromDays(15);

            if (user.TokenValidBefore <= DateTime.UtcNow || user.TokenValidBefore == null)
            {
                user.TokenValidBefore = DateTime.UtcNow.Add((TimeSpan)duration);
                user.Token = refreshToken;
                await _context.SaveChangesAsync();
            }

            return user.Token;
        }

        public async Task<Account?> UpdateConfirmationCode(Guid id, string code)
        {
            var account = await GetById(id);
            if (account == null)
                return null;

            account.ConfirmationCode = code;
            account.ConfirmationCodeValidBefore = DateTime.UtcNow.AddMinutes(5);
            await _context.SaveChangesAsync();

            return account;
        }

        public async Task<Account?> ChangePassword(Guid id, string password, string newPassword)
        {
            var account = await GetById(id);
            if (account == null)
                return null;

            var hashPassword = _passwordHasher.Compute(password);
            if (hashPassword != account.PasswordHash)
                return null;

            account.PasswordHash = _passwordHasher.Compute(newPassword);
            account.LastPasswordDateModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return account;
        }

        public async Task<IEnumerable<Account>> GetAllAccounts(int count, int offset, bool isOrderByAscending = true)
        {
            var query = _context.Accounts.Take(count).Skip(offset);
            if (isOrderByAscending)
                query = query.OrderBy(e => e.CreatedAt);
            else
                query = query.OrderByDescending(e => e.CreatedAt);

            return await query.ToListAsync();
        }

        public async Task<int> GetTotalAccounts() => await _context.Accounts.CountAsync();

        public async Task<bool> RemoveAccount(Guid id)
        {
            var account = await GetById(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> RemoveAccount(string email)
        {
            var account = await GetByEmail(email);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<Account?> AddAsync(string firstName, string lastName, string email, string passwordHash, string role)
        {
            var oldUser = await GetByEmail(email);
            if (oldUser != null)
                return null;

            var newUser = new Account
            {
                Email = email,
                PasswordHash = passwordHash,
                RoleName = role,
                FirstName = firstName,
                LastName = lastName,
                IsEmailVerified = true,
            };

            var result = await _context.Accounts.AddAsync(newUser);
            await _context.SaveChangesAsync();
            return result?.Entity;
        }

        public async Task<Account?> VerifyConfirmationCode(Guid id, string email, string code)
        {
            var account = await GetById(id);
            if (account == null)
                return null;

            if (account.ConfirmationCode != code || account.ConfirmationCodeValidBefore < DateTime.UtcNow)
                return null;

            account.IsEmailVerified = true;
            account.Email = email;
            account.ConfirmationCode = null;
            account.ConfirmationCodeValidBefore = null;

            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<Account?> Update(Guid id, string? firstName, string? lastName, AccountRole? role)
        {
            var account = await GetById(id);
            if (account == null)
                return null;

            if (firstName != null)
                account.FirstName = firstName;

            if (lastName != null)
                account.LastName = lastName;

            if (role != null)
                account.RoleName = role.ToString();

            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<Account?> Update(Guid id, string? firstName, string? lastName)
        {
            var account = await GetById(id);
            if (account == null)
                return null;

            if (firstName != null)
                account.FirstName = firstName;

            if (lastName != null)
                account.LastName = lastName;

            await _context.SaveChangesAsync();
            return account;
        }
    }
}