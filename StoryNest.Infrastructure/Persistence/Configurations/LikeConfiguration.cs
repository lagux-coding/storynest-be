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
    public class LikeConfiguration : IEntityTypeConfiguration<Like>
    {
        public void Configure(EntityTypeBuilder<Like> builder)
        {
            builder.ToTable("Likes");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(l => l.StoryId)
                   .HasColumnName("story_id")
                   .IsRequired();

            builder.Property(l => l.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(l => l.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(l => l.RevokedAt)
                   .HasColumnName("revoked_at");

            // Relation: 1 Story -> nhiều Likes
            builder.HasOne(l => l.Story)
                   .WithMany(s => s.Likes)
                   .HasForeignKey(l => l.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relation: 1 User -> nhiều Likes
            builder.HasOne(l => l.User)
                   .WithMany(u => u.Likes)
                   .HasForeignKey(l => l.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: 1 user chỉ like 1 story 1 lần
            builder.HasIndex(l => new { l.StoryId, l.UserId })
                   .IsUnique();
        }
    }
}
