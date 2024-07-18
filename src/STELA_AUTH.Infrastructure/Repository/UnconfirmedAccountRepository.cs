using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
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
        private readonly IDistributedCache _distributedCache;
        private const string _prefixKey = "unconfirmed_account_";

        public UnconfirmedAccountRepository(
            AuthDbContext context,
            Hmac512Provider passwordHasher,
            IDistributedCache distributedCache)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _distributedCache = distributedCache;
        }

        public async Task<UnconfirmedAccount?> AddAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            string confirmationCode)
        {
            var key = GetCacheKey(email);

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
            await CacheSetAsync(key, account, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(3));

            return account;
        }

        public async Task<UnconfirmedAccount?> GetByEmail(string email)
        {
            var key = GetCacheKey(email);
            var cachedData = await GetFromCacheAsync<UnconfirmedAccount>(key);
            if (cachedData != null)
                return cachedData;

            var account = await _context.UnconfirmedAccounts.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Email == email);
            if (account != null)
                await CacheSetAsync(key, account, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(3));
            return account;
        }

        public async Task<bool> Remove(string email)
        {
            var key = GetCacheKey(email);

            var account = await GetByEmail(email);
            if (account == null)
                return true;

            AttachAccountEntityIfNotAttached(account);
            await _distributedCache.RemoveAsync(key);
            _context.UnconfirmedAccounts.Remove(account);
            await _context.SaveChangesAsync();

            return true;
        }

        private void AttachAccountEntityIfNotAttached(UnconfirmedAccount account)
        {
            var localAccount = _context.UnconfirmedAccounts.Local.FirstOrDefault(e => e.Id == account.Id);
            if (localAccount == null)
                _context.Attach(account);
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