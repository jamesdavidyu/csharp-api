using csharp_api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace csharp_api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {

    }

    public DbSet<UserAccount> UserAccounts { get; set; }
}
