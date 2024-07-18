using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
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
        private readonly IDistributedCache _distributedCache;
        private const string _prefixKey = "account_";

        public AccountRepository(
            AuthDbContext context,
            Hmac512Provider passwordHasher,
            IDistributedCache distributedCache)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _distributedCache = distributedCache;
        }

        public async Task<Account?> GetById(Guid id)
        {
            return await GetAccountAsync(id.ToString(), e => e.Id == id);
        }

        public async Task<Account?> GetByEmail(string email)
        {
            return await GetAccountAsync(email, e => e.Email == email);
        }

        public async Task<Account?> GetByTokenAsync(string refreshTokenHash)
        {
            return await GetAccountAsync($"account_token_{refreshTokenHash}", e => e.Token == refreshTokenHash);
        }

        public async Task<Account?> UpdateImage(Guid accountId, string filename)
        {
            var account = await GetById(accountId);
            if (account == null)
                return null;

            AttachAccountEntityIfNotAttached(account);

            account.Image = filename;
            await _context.SaveChangesAsync();

            var key = GetCacheKey(accountId.ToString());
            await CacheSetAsync(key, account, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));

            return account;
        }

        public async Task<string?> UpdateTokenAsync(string refreshToken, Guid accountId, TimeSpan? duration = null)
        {
            var account = await GetById(accountId);
            if (account == null)
                return null;

            if (duration == null)
                duration = TimeSpan.FromDays(15);

            if (account.TokenValidBefore <= DateTime.UtcNow || account.TokenValidBefore == null)
            {
                AttachAccountEntityIfNotAttached(account);

                account.TokenValidBefore = DateTime.UtcNow.Add((TimeSpan)duration);
                account.Token = refreshToken;
                await _context.SaveChangesAsync();
            }

            var key = GetCacheKey(accountId.ToString());
            await CacheSetAsync(key, account, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));

            return account.Token;
        }

        public async Task<Account?> UpdateConfirmationCode(Guid id, string code)
        {
            var account = await GetById(id);
            if (account == null)
                return null;

            AttachAccountEntityIfNotAttached(account);

            account.ConfirmationCode = code;
            account.ConfirmationCodeValidBefore = DateTime.UtcNow.AddMinutes(5);
            await _context.SaveChangesAsync();

            var key = GetCacheKey(id.ToString());
            await CacheSetAsync(key, account, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));

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

            AttachAccountEntityIfNotAttached(account);

            account.PasswordHash = _passwordHasher.Compute(newPassword);
            account.LastPasswordDateModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return account;
        }

        public async Task<IEnumerable<Account>> GetAllAccounts(int count, int offset, bool isOrderByAscending = true)
        {
            var query = _context.Accounts.Take(count)
                                         .Skip(offset);
            if (isOrderByAscending)
                query = query.OrderBy(e => e.CreatedAt);
            else
                query = query.OrderByDescending(e => e.CreatedAt);

            var key = GetCacheKey($"all{count}_{offset}");
            await CacheSetAsync(key, query.ToList(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2));

            return await query.ToListAsync();
        }

        public async Task<int> GetTotalAccounts()
        {
            var key = "accounts_total";
            var cachedData = await GetFromCacheAsync<int>(key);
            if (cachedData != null)
                return cachedData;

            var total = await _context.Accounts.CountAsync();
            await CacheSetAsync(key, total, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1));
            return total;
        }

        public async Task<Account?> AddAsync(string firstName, string lastName, string email, string passwordHash, string role)
        {
            var account = await GetByEmail(email);
            if (account != null)
                return null;

            account = new Account
            {
                Email = email,
                PasswordHash = passwordHash,
                RoleName = role,
                FirstName = firstName,
                LastName = lastName,
                IsEmailVerified = true,
            };

            var result = await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            var key = GetCacheKey(account.Id.ToString());
            await CacheSetAsync(key, account, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return result?.Entity;
        }

        public async Task<Account?> VerifyConfirmationCode(Guid id, string email, string code)
        {
            var account = await GetById(id);
            if (account == null)
                return null;

            if (account.ConfirmationCode != code || account.ConfirmationCodeValidBefore < DateTime.UtcNow)
                return null;

            AttachAccountEntityIfNotAttached(account);

            account.IsEmailVerified = true;
            account.Email = email;
            account.ConfirmationCode = null;
            account.ConfirmationCodeValidBefore = null;

            await _context.SaveChangesAsync();

            var key = GetCacheKey(account.Id.ToString());
            await CacheSetAsync(key, account, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return account;
        }

        public async Task<Account?> Update(Guid id, string? firstName, string? lastName, AccountRole? role)
        {
            bool isUpdated = false;

            var account = await GetById(id);
            if (account == null)
                return null;

            if (firstName != null)
            {
                account.FirstName = firstName;
                isUpdated = true;
            }

            if (lastName != null)
            {
                account.LastName = lastName;
                isUpdated = true;
            }

            if (role != null)
            {
                account.RoleName = role.ToString();
                isUpdated = true;
            }

            if (isUpdated)
            {
                AttachAccountEntityIfNotAttached(account);

                await _context.SaveChangesAsync();

                var key = GetCacheKey(account.Id.ToString());
                await CacheSetAsync(key, account, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            }
            return account;
        }

        public async Task<Account?> Update(Guid id, string? firstName, string? lastName)
        {
            bool isUpdated = false;

            var account = await GetById(id);
            if (account == null)
                return null;

            if (firstName != null)
            {
                account.FirstName = firstName;
                isUpdated = true;
            }

            if (lastName != null)
            {
                account.LastName = lastName;
                isUpdated = true;
            }

            if (isUpdated)
            {
                AttachAccountEntityIfNotAttached(account);

                await _context.SaveChangesAsync();

                var key = GetCacheKey(account.Id.ToString());
                await CacheSetAsync(key, account, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            }
            return account;
        }

        private void AttachAccountEntityIfNotAttached(Account account)
        {
            var localAccount = _context.Accounts.Local.FirstOrDefault(e => e.Id == account.Id);
            if (localAccount == null)
                _context.Attach(account);
        }

        private async Task<Account?> GetAccountAsync(string keySuffix, Expression<Func<Account, bool>> predicate)
        {
            var key = GetCacheKey(keySuffix);
            var account = await GetFromCacheAsync<Account>(key);
            if (account != null)
                return account;

            account = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(predicate);
            if (account != null)
                await CacheSetAsync(key, account, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return account;
        }

        private async Task CacheSetAsync<T>(string key, T data, TimeSpan slidingExpiration, TimeSpan absoluteExpiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpiration = DateTime.Now.Add(absoluteExpiration)
            };
            var serializedData = JsonSerializer.Serialize(data);
            await _distributedCache.SetStringAsync(key, serializedData, options);
        }

        private async Task<T?> GetFromCacheAsync<T>(string key)
        {
            var cachedData = await _distributedCache.GetStringAsync(key);
            return cachedData != null ? JsonSerializer.Deserialize<T>(cachedData) : default;
        }

        private string GetCacheKey(string identifier) => $"{_prefixKey}{identifier}";
    }
}