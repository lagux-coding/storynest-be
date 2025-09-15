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
    public class AdvertismentConfiguration : IEntityTypeConfiguration<Advertisement>
    {
        public void Configure(EntityTypeBuilder<Advertisement> builder)
        {
            builder.ToTable("Advertisements");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(a => a.Title)
                   .HasColumnName("title")
                   .IsRequired();

            builder.Property(a => a.Content)
                   .HasColumnName("content");

            builder.Property(a => a.MediaUrl)
                   .HasColumnName("media_url");

            builder.Property(a => a.TargetUrl)
                   .HasColumnName("target_url");

            builder.Property(a => a.Placement)
                   .HasColumnName("placement")
                   .HasMaxLength(100);

            builder.Property(a => a.Type)
                   .HasColumnName("type")
                   .HasConversion<string>() 
                   .HasMaxLength(10)
                   .IsRequired();

            builder.Property(a => a.StartDate)
                   .HasColumnName("start_date")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(a => a.EndDate)
                   .HasColumnName("end_date");

            builder.Property(a => a.Impressions)
                   .HasColumnName("impressions")
                   .HasDefaultValue(0);

            builder.Property(a => a.Clicks)
                   .HasColumnName("clicks")
                   .HasDefaultValue(0);

            builder.Property(a => a.Status)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .HasMaxLength(10)
                   .IsRequired();

            builder.Property(a => a.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(a => a.UpdatedAt)
                   .HasColumnName("updated_at");
        }
    }
}
