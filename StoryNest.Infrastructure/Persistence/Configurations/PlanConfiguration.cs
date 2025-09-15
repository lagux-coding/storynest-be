using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Configurations
{
    public class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.ToTable("Plans");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(p => p.Name)
                .HasColumnName("name")
                .IsRequired();

            builder.Property(p => p.Slug)
                .HasColumnName("slug")
                .IsRequired();

            builder.Property(p => p.Description)
                .HasColumnName("description");

            builder.Property(p => p.PriceMonthly)
                .HasColumnName("price_monthly");

            builder.Property(p => p.PriceYearly)
                .HasColumnName("price_yearly");

            builder.Property(p => p.Features)
                .HasColumnName("features")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
                )
                .HasColumnType("jsonb");

            builder.Property(p => p.Currency)
                .HasColumnName("currency")
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(p => p.AiCreditsDaily)
                .HasColumnName("ai_credits_daily");

            builder.Property(p => p.DurationInDays)
                .HasColumnName("duration_in_days");

            builder.Property(p => p.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(p => p.SortOrder)
                .HasColumnName("sort_order")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasMany(p => p.Subscriptions)
                   .WithOne(s => s.Plan)
                   .HasForeignKey(s => s.PlanId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint on Slug
            builder.HasIndex(p => p.Slug).IsUnique();
        }
    }
}
