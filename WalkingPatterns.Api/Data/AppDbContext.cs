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

        public DbSet<ProjectVersionDetails> ProjectVersionDetails { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProjectVersionDetails>()
                .HasOne(project => project.Client)
                .WithMany(client => client.ProjectVersions)
                .HasForeignKey(project => project.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
