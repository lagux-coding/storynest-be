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
    public class FollowConfiguration : IEntityTypeConfiguration<Follow>
    {
        public void Configure(EntityTypeBuilder<Follow> builder)
        {
            builder.ToTable("Follows");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(f => f.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(f => f.FollowerId)
                   .HasColumnName("follower_id")
                   .IsRequired();

            builder.Property(f => f.FollowStatus)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(f => f.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(f => f.UpdatedAt)
                   .HasColumnName("updated_at");

            // Relation: User được follow
            builder.HasOne(f => f.User)
                   .WithMany(u => u.Follows)
                   .HasForeignKey(f => f.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relation: User đi follow
            builder.HasOne(f => f.Follower)
                   .WithMany(u => u.Following)
                   .HasForeignKey(f => f.FollowerId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: một user chỉ follow 1 user khác 1 lần
            builder.HasIndex(f => new { f.UserId, f.FollowerId })
                   .IsUnique();
        }
    }
}
