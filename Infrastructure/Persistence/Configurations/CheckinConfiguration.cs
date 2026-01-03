using Domain.Entity;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CheckinConfiguration : IEntityTypeConfiguration<Checkin>
    {
        public void Configure(EntityTypeBuilder<Checkin> builder)
        {
            builder.ToTable("Checkins");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CampaignId)
                .IsRequired();

            builder.Property(c => c.CheckinTime)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.Method)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(c => c.Location)
                .HasMaxLength(int.MaxValue);

            builder.Property(c => c.Success)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.Notes)
                .HasMaxLength(500);

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(c => c.Campaign)
                .WithMany(c => c.Checkins)
                .HasForeignKey(c => c.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

