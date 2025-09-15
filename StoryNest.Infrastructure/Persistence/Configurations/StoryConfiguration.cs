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
    public class StoryConfiguration : IEntityTypeConfiguration<Story>
    {
        public void Configure(EntityTypeBuilder<Story> builder)
        {
            builder.ToTable("Stories");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(s => s.Title)
                   .HasColumnName("title")
                   .HasMaxLength(300)
                   .IsRequired();

            builder.Property(s => s.Slug)
                   .HasColumnName("slug")
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(s => s.Content)
                   .HasColumnName("content")
                   .IsRequired();

            builder.Property(s => s.Summary)
                   .HasColumnName("summary")
                   .HasMaxLength(1000);

            builder.Property(s => s.CoverImageUrl)
                   .HasColumnName("cover_image_url");

            builder.Property(s => s.PrivacyStatus)
                   .HasColumnName("privacy_status")
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(s => s.StoryStatus)
                   .HasColumnName("story_status")
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(s => s.ViewCount)
                   .HasColumnName("view_count")
                   .HasDefaultValue(0);

            builder.Property(s => s.LikeCount)
                   .HasColumnName("like_count")
                   .HasDefaultValue(0);

            builder.Property(s => s.CommentCount)
                   .HasColumnName("comment_count")
                   .HasDefaultValue(0);

            builder.Property(s => s.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(s => s.LastUpdatedAt)
                   .HasColumnName("last_updated_at");

            builder.Property(s => s.PublishedAt)
                   .HasColumnName("published_at");

            // --- Relations ---

            // 1 Story -> nhiều Media
            builder.HasMany(s => s.Media)
                   .WithOne(m => m.Story)
                   .HasForeignKey(m => m.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1 Story -> nhiều CollectionStories (n-n Collection)
            builder.HasMany(s => s.CollectionStories)
                   .WithOne(cs => cs.Story)
                   .HasForeignKey(cs => cs.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1 Story -> nhiều Comments
            builder.HasMany(s => s.Comments)
                   .WithOne(c => c.Story)
                   .HasForeignKey(c => c.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1 Story -> nhiều Likes
            builder.HasMany(s => s.Likes)
                   .WithOne(l => l.Story)
                   .HasForeignKey(l => l.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1 Story -> nhiều StoryViews
            builder.HasMany(s => s.StoryViews)
                   .WithOne(v => v.Story)
                   .HasForeignKey(v => v.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1 Story -> nhiều Reports
            builder.HasMany(s => s.Reports)
                   .WithOne(r => r.Content)
                   .HasForeignKey(r => r.ReportedStoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.StoryTags)
                   .WithOne(st => st.Story)
                   .HasForeignKey(st => st.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
