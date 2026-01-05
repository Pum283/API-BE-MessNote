using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PasswordResetOTPConfiguration : IEntityTypeConfiguration<PasswordResetOTP>
    {
        public void Configure(EntityTypeBuilder<PasswordResetOTP> builder)
        {
            builder.ToTable("PasswordResetOTPs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.OTPCode)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.AttemptCount)
                .IsRequired()
                .HasDefaultValue(0);

            // Index for faster lookups
            builder.HasIndex(x => x.Email);
            builder.HasIndex(x => new { x.Email, x.OTPCode, x.IsUsed });
        }
    }
}

