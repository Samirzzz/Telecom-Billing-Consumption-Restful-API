using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.Data
{
    public class TelecomBillingDbContext : DbContext
    {
        public TelecomBillingDbContext(DbContextOptions<TelecomBillingDbContext> options) : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<UsageRecord> UsageRecords { get; set; }
        public DbSet<TariffRule> TariffRules { get; set; }
        public DbSet<BundleLimit> BundleLimits { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
                
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
                
            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();
            
            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();
                
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Invoice configuration
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.User)
                .WithMany(u => u.Invoices)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => new { i.UserId, i.Month })
                .IsUnique();
            
            // UsageRecord configuration
            modelBuilder.Entity<UsageRecord>()
                .HasOne(u => u.User)
                .WithMany(user => user.UsageRecords)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<UsageRecord>()
                .HasIndex(u => new { u.UserId, u.Timestamp });
            
            // BundleLimit configuration
            modelBuilder.Entity<BundleLimit>()
                .HasIndex(bl => bl.PlanType)
                .IsUnique();
        }
    }
}
