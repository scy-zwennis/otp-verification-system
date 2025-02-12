using Microsoft.EntityFrameworkCore;
using OtpVerification.Api.Data.Entities;

namespace OtpVerification.Api.Data;

public class OtpDbContext : DbContext
{
    public OtpDbContext(DbContextOptions<OtpDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(entity => {
            entity.ToTable("Users");

            entity.HasKey(e => e.UserId);

            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW()");
        });
    }

    public required DbSet<User> Users { get; set; }
}