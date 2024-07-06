using Microsoft.EntityFrameworkCore;
using STELA_AUTH.Core.Entities.Models;

namespace STELA_AUTH.Infrastructure.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<UnconfirmedAccount> UnconfirmedAccounts { get; set; }
    }
}