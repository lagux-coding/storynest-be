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
    public class MediaConfiguration : IEntityTypeConfiguration<Media>
    {
        public void Configure(EntityTypeBuilder<Media> builder)
        {
            builder.ToTable("Media");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(m => m.StoryId)
                   .HasColumnName("story_id")
                   .IsRequired();

            builder.Property(m => m.MediaUrl)
                   .HasColumnName("media_url")
                   .IsRequired();

            builder.Property(m => m.MediaType)
                   .HasColumnName("media_type")
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(m => m.Caption)
                   .HasColumnName("caption")
                   .HasMaxLength(500);

            builder.Property(m => m.MimeType)
                   .HasColumnName("mime_type")
                   .HasMaxLength(100);

            builder.Property(m => m.Size)
                   .HasColumnName("size");

            builder.Property(m => m.Width)
                   .HasColumnName("width");

            builder.Property(m => m.Height)
                   .HasColumnName("height");

            builder.Property(m => m.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(m => m.UpdatedAt)
                   .HasColumnName("updated_at");

            // Relation: 1 Story -> nhiều Media
            builder.HasOne(m => m.Story)
                   .WithMany(s => s.Media)
                   .HasForeignKey(m => m.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
