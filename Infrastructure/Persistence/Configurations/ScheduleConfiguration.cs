using Domain.Entity;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
    {
        public void Configure(EntityTypeBuilder<Schedule> builder)
        {
            builder.ToTable("Schedules");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.CampaignId)
                .IsRequired();

            builder.Property(s => s.ScheduleType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(s => s.FixedTime);

            builder.Property(s => s.CheckinInterval)
                .HasMaxLength(50);

            builder.Property(s => s.OverdueThreshold)
                .HasMaxLength(50);

            builder.Property(s => s.Recurring)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(RecurringType.None);

            builder.Property(s => s.LastCheckinTime);

            builder.Property(s => s.NextSendTime);

            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(s => s.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(s => s.Campaign)
                .WithOne(c => c.Schedule)
                .HasForeignKey<Schedule>(s => s.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

