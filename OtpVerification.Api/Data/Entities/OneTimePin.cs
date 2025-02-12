using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OtpVerification.Api.Data.Entities;

public class OneTimePin
{
    public int OneTimePinId { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int RequestCount { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }

    public User User { get; set; } = null!;

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public void SetExpiryDt(int expiresInSeconds)
    {
        ExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);
    }

    public void Expire()
    {
        IsUsed = true;
    }

    public void Use()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }

    public static OneTimePin Create(int userId, string code, int expiresInSeconds)
    {
        var e = new OneTimePin {
            UserId = userId,
            Code = code,
            RequestCount = 1 
        };

        e.SetExpiryDt(expiresInSeconds);
        return e;
    }

    public static void ConfigureModel(EntityTypeBuilder<OneTimePin> entity)
    {
        entity.ToTable("OneTimePins");

        entity.HasKey(e => e.OneTimePinId);

        entity.Property(e => e.UserId)
            .IsRequired();

        entity.Property(e => e.Code)
            .HasMaxLength(6)
            .IsRequired();

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("NOW()");

        entity.Property(e => e.RequestCount)
            .HasDefaultValue(0);

        entity.Property(e => e.IsUsed)
            .HasDefaultValue(false);

        entity.HasIndex(e => new { e.UserId, e.CreatedAt });
    }
}