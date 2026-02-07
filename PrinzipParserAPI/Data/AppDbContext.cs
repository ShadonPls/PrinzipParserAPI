using Microsoft.EntityFrameworkCore;
using PrinzipParserAPI.Models;
using System.Collections.Generic;

namespace PrinzipParserAPI.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Subscription> Subscriptions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.LastStatus)
                    .HasMaxLength(100);
                entity.Property(e => e.LastPrice)
                    .HasPrecision(18, 2);

                entity.HasIndex(e => e.ApartmentId);
            });
        }

    }

}
