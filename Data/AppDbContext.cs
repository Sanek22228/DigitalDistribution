using DigitalDistribution.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalDistribution.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Key> Keys { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Key>()
                .Property(c => c.Status)
                .HasConversion<int>();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            base.OnModelCreating(modelBuilder);
        }
    }
}
