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
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

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
                   .HasDefaultValueSql("CURRENT_TIMESTAMP")
                   .IsRequired();

            builder.Property(u => u.UpdatedAt)
                   .HasColumnName("updated_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP")
                   .IsRequired();

            builder.Property(u => u.IsActive)
                   .HasColumnName("is_active");

            // Unique constraints
            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();

            // Refreshtoken (1 - n)
            builder.HasMany(u => u.RefreshTokens)
                   .WithOne(rt => rt.User)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Subscription (1 - n)
            builder.HasMany(u => u.Subscriptions)
                   .WithOne(s => s.User)
                   .HasForeignKey(s => s.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // AITransactions (1-n)
            builder.HasMany(u => u.AITransactions)
                   .WithOne(at => at.User)
                   .HasForeignKey(at => at.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Likes (1-n)
            builder.HasMany(u => u.Likes)
                   .WithOne(l => l.User)
                   .HasForeignKey(l => l.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Comments (1-n)
            builder.HasMany(u => u.Comments)
                   .WithOne(c => c.User)
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Follows - Who follow me
            builder.HasMany(u => u.Follows)
                   .WithOne(f => f.User) // User được follow
                   .HasForeignKey(f => f.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Follows - Who I follow
            builder.HasMany(u => u.Following)
                   .WithOne(f => f.Follower) // User là follower
                   .HasForeignKey(f => f.FollowerId)
                   .OnDelete(DeleteBehavior.Cascade);

            // AIUsageLogs (1-n)
            builder.HasMany(u => u.AIUsageLogs)
                   .WithOne(log => log.User)
                   .HasForeignKey(log => log.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Collections (1-n)
            builder.HasMany(u => u.Collections)
                   .WithOne(c => c.User)
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Notifications - received
            builder.HasMany(u => u.Notifications)
                   .WithOne(n => n.User) // user nhận
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Notifications - acted by this user
            builder.HasMany(u => u.ActorNotifications)
                   .WithOne(n => n.Actor) // user gây ra
                   .HasForeignKey(n => n.ActorId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Payments (1-n)
            builder.HasMany(u => u.Payments)
                   .WithOne(p => p.User)
                   .HasForeignKey(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // StoryViews (1-n)
            builder.HasMany(u => u.StoryViews)
                   .WithOne(sv => sv.User)
                   .HasForeignKey(sv => sv.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1-1 AICredit
            builder.HasOne(u => u.AICredit)
                   .WithOne(ac => ac.User)
                   .HasForeignKey<AICredit>(ac => ac.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
