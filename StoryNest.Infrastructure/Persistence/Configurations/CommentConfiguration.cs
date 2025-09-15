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
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(c => c.StoryId)
                   .HasColumnName("story_id")
                   .IsRequired();

            builder.Property(c => c.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(c => c.ParentCommentId)
                   .HasColumnName("parent_comment_id");

            builder.Property(c => c.Content)
                   .HasColumnName("content")
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(c => c.CommentStatus)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(c => c.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(c => c.UpdatedAt)
                   .HasColumnName("updated_at");

            builder.Property(c => c.DeletedAt)
                   .HasColumnName("deleted_at");

            // Relation: 1 Story -> nhiều Comments
            builder.HasOne(c => c.Story)
                   .WithMany(s => s.Comments)
                   .HasForeignKey(c => c.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relation: 1 User -> nhiều Comments
            builder.HasOne(c => c.User)
                   .WithMany(u => u.Comments)
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Self-referencing (ParentComment -> Replies)
            builder.HasOne<Comment>()
                   .WithMany()
                   .HasForeignKey(c => c.ParentCommentId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
