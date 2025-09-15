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
    public class CollectionStoryConfiguration : IEntityTypeConfiguration<CollectionStory>
    {
        public void Configure(EntityTypeBuilder<CollectionStory> builder)
        {
            builder.ToTable("CollectionStories");

            builder.HasKey(cs => cs.Id);

            builder.Property(cs => cs.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(cs => cs.CollectionId)
                   .HasColumnName("collection_id")
                   .IsRequired();

            builder.Property(cs => cs.StoryId)
                   .HasColumnName("story_id")
                   .IsRequired();

            builder.Property(cs => cs.AddedAt)
                   .HasColumnName("added_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relation: Collection (1-n)
            builder.HasOne(cs => cs.Collection)
                   .WithMany(c => c.CollectionStories)
                   .HasForeignKey(cs => cs.CollectionId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relation: Story (1-n)
            builder.HasOne(cs => cs.Story)
                   .WithMany(s => s.CollectionStories)
                   .HasForeignKey(cs => cs.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: 1 Story chỉ được thêm 1 lần vào 1 Collection
            builder.HasIndex(cs => new { cs.CollectionId, cs.StoryId })
                   .IsUnique();
        }
    }
}
