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
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(n => n.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(n => n.ActorId)
                   .HasColumnName("actor_id")
                   .IsRequired();

            builder.Property(n => n.ReferenceId)
                   .HasColumnName("reference_id");

            builder.Property(n => n.Content)
                   .HasColumnName("content")
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(n => n.Type)
                   .HasColumnName("type")
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(n => n.IsRead)
                   .HasColumnName("is_read")
                   .HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relation: User nhận thông báo
            builder.HasOne(n => n.User)
                   .WithMany(u => u.Notifications)
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relation: User gây ra hành động
            builder.HasOne(n => n.Actor)
                   .WithMany(u => u.ActorNotifications)
                   .HasForeignKey(n => n.ActorId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
