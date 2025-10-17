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
    public class UserReportConfiguration : IEntityTypeConfiguration<UserReport>
    {
        public void Configure(EntityTypeBuilder<UserReport> builder)
        {
            builder.ToTable("UserReports");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(r => r.ReportedId)
                   .HasColumnName("reported_id")
                   .IsRequired();

            builder.Property(r => r.ReportedStoryId)
                   .HasColumnName("reported_story_id");

            builder.Property(r => r.ReportedCommentId)
                   .HasColumnName("reported_comment_id");

            builder.Property(r => r.AdminId)
                   .HasColumnName("admin_id");

            builder.Property(r => r.Reason)
                   .HasColumnName("reason")
                   .HasMaxLength(1000)
                   .IsRequired();

            builder.Property(r => r.Status)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(r => r.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(r => r.UpdatedAt)
                   .HasColumnName("updated_at");

            // --- Relations ---

            // Reporter (User who created the report) -> nhưng đang dùng ReportedId field
            builder.HasOne(r => r.Reporter)
                   .WithMany(u => u.ReportsCreated)
                   .HasForeignKey(r => r.ReporterId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ReportedUser)
                   .WithMany(u => u.ReportsReceived)
                   .HasForeignKey(r => r.ReportedId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Reported story
            builder.HasOne(r => r.ReportedStory)
                   .WithMany(s => s.Reports)
                   .HasForeignKey(r => r.ReportedStoryId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(r => r.ReportedComment)
                   .WithMany(c => c.Reports)
                   .HasForeignKey(r => r.ReportedCommentId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Admin xử lý report
            builder.HasOne(r => r.Admin)
                   .WithMany()
                   .HasForeignKey(r => r.AdminId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
