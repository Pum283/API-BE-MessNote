using Domain.Entity;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
    {
        public void Configure(EntityTypeBuilder<Campaign> builder)
        {
            builder.ToTable("Campaigns");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.UserId)
                .IsRequired();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(int.MaxValue);

            builder.Property(c => c.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(CampaignStatus.Active);

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(c => c.User)
                .WithMany(u => u.Campaigns)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Schedule)
                .WithOne(s => s.Campaign)
                .HasForeignKey<Schedule>(s => s.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Recipients)
                .WithOne(r => r.Campaign)
                .HasForeignKey(r => r.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.MessageTemplates)
                .WithOne(m => m.Campaign)
                .HasForeignKey(m => m.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Checkins)
                .WithOne(ch => ch.Campaign)
                .HasForeignKey(ch => ch.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Histories)
                .WithOne(h => h.Campaign)
                .HasForeignKey(h => h.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

