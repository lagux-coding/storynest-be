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
    public class AICreditConfiguration : IEntityTypeConfiguration<AICredit>
    {
        public void Configure(EntityTypeBuilder<AICredit> builder)
        {
            builder.ToTable("AICredits");

            builder.HasKey(ac => ac.Id);

            builder.Property(ac => ac.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(ac => ac.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(ac => ac.TotalCredits)
                   .HasColumnName("total_credits")
                   .HasDefaultValue(0);

            builder.Property(ac => ac.UsedCredits)
                   .HasColumnName("used_credits")
                   .HasDefaultValue(0);

            // RemainingCredits là computed property => không map cột
            builder.Ignore(ac => ac.RemainingCredits);

            builder.Property(ac => ac.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(ac => ac.UpdatedAt)
                   .HasColumnName("updated_at");

            builder.Property(ac => ac.LastRenewDate)
                   .HasColumnName("last_renew_date");

            // 1-1 relation với User
            builder.HasOne(ac => ac.User)
                   .WithOne(u => u.AICredit)
                   .HasForeignKey<AICredit>(ac => ac.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
