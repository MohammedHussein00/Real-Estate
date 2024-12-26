using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Real_State.Tables;
using System;

namespace Real_State.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure many-to-many relationship
            modelBuilder.Entity<UserProperty>()
                .HasKey(up => new { up.UserId, up.PropertyId }); // Composite primary key

            modelBuilder.Entity<UserProperty>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProperties)
                .HasForeignKey(up => up.UserId);

            modelBuilder.Entity<UserProperty>()
                .HasOne(up => up.Property)
                .WithMany(p => p.UserProperties)
                .HasForeignKey(up => up.PropertyId);
        }
        public DbSet<Property> Properties { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserProperty> UserProperties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
    }
}
