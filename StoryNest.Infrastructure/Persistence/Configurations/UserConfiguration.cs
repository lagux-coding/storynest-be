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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .HasColumnName("id")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(u => u.Username)
                   .HasColumnName("username")
                   .IsRequired();

            builder.Property(u => u.Email)
                   .HasColumnName("email")
                   .IsRequired();

            builder.Property(u => u.PasswordHash)
                   .HasColumnName("password_hash")
                   .IsRequired();

            builder.Property(u => u.FullName)
                   .HasColumnName("full_name");

            builder.Property(u => u.Bio)
                   .HasColumnName("bio");

            builder.Property(u => u.AvatarUrl)
                   .HasColumnName("avatar_url");

            builder.Property(u => u.CoverUrl)
                   .HasColumnName("cover_url");

            builder.Property(u => u.DateOfBirth)
                   .HasColumnName("date_of_birth");

            builder.Property(u => u.Gender)
                   .HasColumnName("gender");

            builder.Property(u => u.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC' + interval '7 hour'")
                   .IsRequired();

            builder.Property(u => u.UpdatedAt)
                   .HasColumnName("updated_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC' + interval '7 hour'")
                   .IsRequired();

            builder.Property(u => u.IsActive)
                   .HasColumnName("is_active");

            // Unique constraints
            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
        }
    }
}
