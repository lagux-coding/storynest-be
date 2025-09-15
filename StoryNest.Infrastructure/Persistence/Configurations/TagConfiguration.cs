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
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tags");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(t => t.Name)
                   .HasColumnName("name")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(t => t.Slug)
                   .HasColumnName("slug")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(t => t.Description)
                   .HasColumnName("description")
                   .HasMaxLength(500);

            builder.Property(t => t.Color)
                   .HasColumnName("color")
                   .HasMaxLength(20);

            builder.Property(t => t.IconUrl)
                   .HasColumnName("icon_url");

            builder.Property(t => t.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(t => t.UpdatedAt)
                   .HasColumnName("updated_at");

            builder.Property(t => t.DeletedAt)
                   .HasColumnName("deleted_at");

            // Relation: 1 Tag -> nhiều StoryTags (n-n Story)
            builder.HasMany(t => t.StoryTags)
                   .WithOne(st => st.Tag)
                   .HasForeignKey(st => st.TagId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Unique constraints
            builder.HasIndex(t => t.Slug).IsUnique();
            builder.HasIndex(t => t.Name).IsUnique();
        }
    }
}
