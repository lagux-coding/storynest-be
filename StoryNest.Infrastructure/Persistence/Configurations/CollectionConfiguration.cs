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
    public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
    {
        public void Configure(EntityTypeBuilder<Collection> builder)
        {
            builder.ToTable("Collections");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(c => c.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(c => c.Description)
                   .HasColumnName("description")
                   .HasMaxLength(1000);

            builder.Property(c => c.CoverImageUrl)
                   .HasColumnName("cover_image_url");

            builder.Property(c => c.IsPublic)
                   .HasColumnName("is_public")
                   .HasDefaultValue(true);

            builder.Property(c => c.StoryCount)
                   .HasColumnName("story_count")
                   .HasDefaultValue(0);

            builder.Property(c => c.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(c => c.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(c => c.UpdatedAt)
                   .HasColumnName("updated_at");

            builder.Property(c => c.DeletedAt)
                   .HasColumnName("deleted_at");

            // Relation: 1 User -> nhiều Collections
            builder.HasOne(c => c.User)
                   .WithMany(u => u.Collections)
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relation: 1 Collection -> nhiều CollectionStories
            builder.HasMany(c => c.CollectionStories)
                   .WithOne(cs => cs.Collection)
                   .HasForeignKey(cs => cs.CollectionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
