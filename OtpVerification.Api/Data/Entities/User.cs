using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OtpVerification.Api.Data.Entities;

public class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int? LastIssuedOtpId { get; set; }

    public ICollection<OneTimePin> OneTimePins { get; set; }
    public OneTimePin? LastIssuedOtp { get; set; }

    public User()
    {
        OneTimePins = new HashSet<OneTimePin>();
    }

    public static User Create(string email)
    {
        return new User { Email = email };
    }

    public static void ConfigureModel(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("Users");

        entity.HasKey(e => e.UserId);

        entity.HasIndex(e => e.Email).IsUnique();

        entity.Property(e => e.Email)
            .HasMaxLength(256)
            .IsRequired();

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("NOW()");

        entity.Property(e => e.LastIssuedOtpId)
            .IsRequired(false);

        entity.HasOne(e => e.LastIssuedOtp)
            .WithOne()
            .HasForeignKey<User>(e => e.LastIssuedOtpId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasMany(e => e.OneTimePins)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}