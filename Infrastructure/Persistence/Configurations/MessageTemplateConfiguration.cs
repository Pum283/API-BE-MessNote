using Domain.Entity;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate>
    {
        public void Configure(EntityTypeBuilder<MessageTemplate> builder)
        {
            builder.ToTable("MessageTemplates");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.CampaignId)
                .IsRequired();

            builder.Property(m => m.Content)
                .IsRequired()
                .HasMaxLength(int.MaxValue);

            builder.Property(m => m.Channel)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(m => m.SequenceOrder)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(m => m.Placeholders)
                .HasMaxLength(int.MaxValue);

            builder.Property(m => m.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(m => m.Campaign)
                .WithMany(c => c.MessageTemplates)
                .HasForeignKey(m => m.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

