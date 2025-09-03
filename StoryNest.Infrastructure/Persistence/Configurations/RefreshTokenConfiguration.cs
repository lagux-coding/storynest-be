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
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokens>
    {
        public void Configure(EntityTypeBuilder<RefreshTokens> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(rt => rt.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(rt => rt.TokenHash)
                   .HasColumnName("tokenHash")
                   .IsRequired()
                   .HasMaxLength(512);

            builder.Property(x => x.JwtId)
                    .IsRequired()
                    .HasMaxLength(64);

            builder.Property(rt => rt.ExpiresAt)
                   .HasColumnName("expires_at")
                   .IsRequired();

            builder.Property(rt => rt.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP")
                   .IsRequired();

            builder.Property(rt => rt.RevokedAt)
                   .HasColumnName("revoked_at");

            builder.Property(rt => rt.ReplacedByTokenHash)
                    .HasColumnName("replaced_by_token_hash")
                    .HasMaxLength(512);

            builder.Property(rt => rt.DeviceId)
                    .HasColumnName("device_id")
                    .HasMaxLength(256);

            builder.Property(rt => rt.IpAddress)
                    .HasColumnName("ip_address")
                    .HasMaxLength(45);

            builder.Property(rt => rt.UserAgent)
                    .HasColumnName("user_agent")
                    .HasMaxLength(512);

            builder.HasOne(rt => rt.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(rt => rt.TokenHash).IsUnique();
            builder.HasIndex(rt => rt.UserId);
        }
    }
   
}
