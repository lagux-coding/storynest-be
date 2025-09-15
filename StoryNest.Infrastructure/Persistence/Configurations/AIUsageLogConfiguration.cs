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
    public class AIUsageLogConfiguration : IEntityTypeConfiguration<AIUsageLog>
    {
        public void Configure(EntityTypeBuilder<AIUsageLog> builder)
        {
            builder.ToTable("AIUsageLogs");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(l => l.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(l => l.UsageFeature)
                   .HasColumnName("usage_feature")
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(l => l.InputToken)
                   .HasColumnName("input_token")
                   .HasDefaultValue(0);

            builder.Property(l => l.OutputToken)
                   .HasColumnName("output_token")
                   .HasDefaultValue(0);

            builder.Property(l => l.CreditUsed)
                   .HasColumnName("credit_used")
                   .HasDefaultValue(0);

            builder.Property(l => l.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relation: 1 User -> nhiều AIUsageLogs
            builder.HasOne(l => l.User)
                   .WithMany(u => u.AIUsageLogs)
                   .HasForeignKey(l => l.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
