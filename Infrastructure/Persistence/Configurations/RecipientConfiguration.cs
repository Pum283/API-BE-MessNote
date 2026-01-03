using Domain.Entity; 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RecipientConfiguration : IEntityTypeConfiguration<Recipient>
    {
        public void Configure(EntityTypeBuilder<Recipient> builder)
        {
            builder.ToTable("Recipients");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.CampaignId)
                .IsRequired();

            builder.Property(r => r.Name)
                .HasMaxLength(100);

            builder.Property(r => r.Phone)
                .HasMaxLength(20);

            builder.Property(r => r.Email)
                .HasMaxLength(100);

            builder.Property(r => r.FacebookId)
                .HasMaxLength(50);

            builder.Property(r => r.Level)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(r => r.Campaign)
                .WithMany(c => c.Recipients)
                .HasForeignKey(r => r.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Histories)
                .WithOne(h => h.Recipient)
                .HasForeignKey(h => h.RecipientId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

