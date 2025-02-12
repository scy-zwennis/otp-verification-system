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
        builder.Entity<User>(User.ConfigureModel);
        builder.Entity<OneTimePin>(OneTimePin.ConfigureModel);
    }

    public required DbSet<User> Users { get; set; }
    public required DbSet<OneTimePin> OneTimePins { get; set; }
}