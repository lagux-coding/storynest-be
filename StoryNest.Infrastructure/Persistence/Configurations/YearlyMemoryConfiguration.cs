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
    public class YearlyMemoryConfiguration : IEntityTypeConfiguration<YearlyMemory>
    {
        public void Configure(EntityTypeBuilder<YearlyMemory> builder)
        {
            builder.ToTable("YearlyMemories");

            builder.HasKey(ym => ym.Id);

            builder.Property(ym => ym.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(ym => ym.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(ym => ym.Year)
                   .HasColumnName("year")
                   .IsRequired();

            builder.Property(ym => ym.MediaUrl)
                   .HasColumnName("media_url")
                   .HasMaxLength(500)
                   .IsRequired();

            builder.Property(ym => ym.Status)
                   .HasColumnName("status")
                   .HasConversion<string>() // enum -> int
                   .IsRequired();

            builder.Property(ym => ym.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(ym => ym.UpdatedAt)
                   .HasColumnName("updated_at");

            // Relation: 1 User -> nhiều YearlyMemories
            builder.HasOne(ym => ym.User)
                   .WithMany(u => u.YearlyMemories)
                   .HasForeignKey(ym => ym.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: mỗi user chỉ có 1 YearlyMemory cho 1 năm
            builder.HasIndex(ym => new { ym.UserId, ym.Year })
                   .IsUnique();
        }
    }
}
