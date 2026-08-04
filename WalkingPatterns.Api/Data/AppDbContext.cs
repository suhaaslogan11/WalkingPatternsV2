using Microsoft.EntityFrameworkCore;
using WalkingPatterns.Api.Models;

namespace WalkingPatterns.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
    }
}