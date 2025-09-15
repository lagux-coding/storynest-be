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
    public class StoryViewConfiguration : IEntityTypeConfiguration<StoryView>
    {
        public void Configure(EntityTypeBuilder<StoryView> builder)
        {
            builder.ToTable("StoryViews");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(v => v.StoryId)
                   .HasColumnName("story_id")
                   .IsRequired();

            builder.Property(v => v.UserId)
                   .HasColumnName("user_id");

            builder.Property(v => v.IpAddress)
                   .HasColumnName("ip_address")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(v => v.DeviceInfo)
                   .HasColumnName("device_info")
                   .HasMaxLength(500);

            builder.Property(v => v.ViewedAt)
                   .HasColumnName("viewed_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relation: 1 Story -> nhiều StoryViews
            builder.HasOne(v => v.Story)
                   .WithMany(s => s.StoryViews)
                   .HasForeignKey(v => v.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relation: 1 User -> nhiều StoryViews (nullable, guest view thì null)
            builder.HasOne(v => v.User)
                   .WithMany(u => u.StoryViews)
                   .HasForeignKey(v => v.UserId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
