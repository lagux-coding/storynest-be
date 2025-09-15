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
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.ToTable("Admins");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Username)
                .HasColumnName("username")
                .IsRequired();

            builder.Property(a => a.PasswordHash)
                .HasColumnName("password_hash")
                .IsRequired();

            builder.Property(a => a.Email)
                .HasColumnName("email")
                .IsRequired();

            builder.Property(a => a.FullName)
                .HasColumnName("full_name");

            builder.Property(a => a.AvatarUrl)
                .HasColumnName("avatar_url");

            builder.Property(a => a.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(a => a.IsSuperAdmin)
                .HasColumnName("is_super_admin")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(a => a.LastLoginAt)
                .HasColumnName("last_login_at");

            builder.Property(a => a.LastLoginIp)
                .HasColumnName("last_login_ip");

            builder.Property(a => a.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(a => a.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Unique constraints
            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();

            // UserReport (1 - n)
            builder.HasMany(a => a.ReportsHandled)
                   .WithOne(r => r.Admin)
                   .HasForeignKey(r => r.AdminId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
