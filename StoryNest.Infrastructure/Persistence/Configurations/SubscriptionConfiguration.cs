using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Configurations
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.ToTable("Subscriptions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(s => s.PlanId)
                   .HasColumnName("plan_id")
                   .IsRequired();

            builder.Property(s => s.StartDate)
                   .HasColumnName("start_date")
                   .IsRequired();

            builder.Property(s => s.EndDate)
                    .HasColumnName("end_date")
                   .IsRequired();

            builder.Property(s => s.Status)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .HasMaxLength(10)
                   .IsRequired();

            builder.Property(s => s.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP")
                   .IsRequired();

            builder.Property(s => s.UpdatedAt)
                   .HasColumnName("updated_at")
                   .IsRequired(false);

            // Unique constraint
            builder.HasIndex(s => new { s.UserId, s.PlanId })
                   .IsUnique()
                   .HasFilter("\"status\" = 'Active'");

            // Relationships
            builder.HasOne(s => s.User)
                   .WithMany(u => u.Subscriptions)
                   .HasForeignKey(s => s.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Plan)
                   .WithMany(p => p.Subscriptions)
                   .HasForeignKey(s => s.PlanId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Payments)
                   .WithOne(p => p.Subscription)
                   .HasForeignKey(p => p.SubscriptionId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
