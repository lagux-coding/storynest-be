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
    public class StoryTagConfiguration : IEntityTypeConfiguration<StoryTag>
    {
        public void Configure(EntityTypeBuilder<StoryTag> builder)
        {
            builder.ToTable("StoryTags");

            builder.HasKey(st => st.Id);

            builder.Property(st => st.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(st => st.StoryId)
                   .HasColumnName("story_id")
                   .IsRequired();

            builder.Property(st => st.TagId)
                   .HasColumnName("tag_id")
                   .IsRequired();

            builder.Property(st => st.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relation: Story (1-n)
            builder.HasOne(st => st.Story)
                   .WithMany(s => s.StoryTags)
                   .HasForeignKey(st => st.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relation: Tag (1-n)
            builder.HasOne(st => st.Tag)
                   .WithMany(t => t.StoryTags)
                   .HasForeignKey(st => st.TagId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: 1 Story không thể có cùng 1 Tag nhiều lần
            builder.HasIndex(st => new { st.StoryId, st.TagId })
                   .IsUnique();
        }
    }
}
