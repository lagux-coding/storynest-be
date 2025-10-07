using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Configurations
{
    public class UserMediaConfiguration : IEntityTypeConfiguration<UserMedia>
    {
        public void Configure(EntityTypeBuilder<UserMedia> builder)
        {
            builder.ToTable("UserMedia");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(x => x.UserId)
                   .IsRequired();

            builder.Property(x => x.MediaType)
                   .HasConversion<string>() // enum -> int
                   .IsRequired();

            builder.Property(x => x.MediaUrl)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasDefaultValue(UserMediaStatus.Pending)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("NOW()");

            builder.Property(x => x.UpdatedAt)
                   .HasDefaultValueSql("NOW()");

            // Relationship
            builder.HasOne(x => x.User)
                   .WithMany(u => u.UserMedias) 
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
