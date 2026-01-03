using Domain.Entity;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class HistoryConfiguration : IEntityTypeConfiguration<History>
    {
        public void Configure(EntityTypeBuilder<History> builder)
        {
            builder.ToTable("Histories");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.CampaignId)
                .IsRequired();

            builder.Property(h => h.EventType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(h => h.Timestamp)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(h => h.RecipientId);

            builder.Property(h => h.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(h => h.Details)
                .HasMaxLength(int.MaxValue);

            builder.Property(h => h.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // === SỬA TẠI ĐÂY ===
            builder.HasOne(h => h.Campaign)
                .WithMany(c => c.Histories)
                .HasForeignKey(h => h.CampaignId)
                .OnDelete(DeleteBehavior.NoAction);  // Đổi từ Cascade thành NoAction

            // Giữ nguyên cái này (hợp lý cho lịch sử)
            builder.HasOne(h => h.Recipient)
                .WithMany(r => r.Histories)
                .HasForeignKey(h => h.RecipientId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}