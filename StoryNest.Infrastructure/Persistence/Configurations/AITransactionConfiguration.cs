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
    public class AITransactionConfiguration : IEntityTypeConfiguration<AITransaction>
    {
        public void Configure(EntityTypeBuilder<AITransaction> builder)
        {
            builder.ToTable("AITransactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(t => t.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(t => t.Type)
                   .HasColumnName("type")
                   .HasConversion<string>() 
                   .IsRequired();

            builder.Property(t => t.Amount)
                   .HasColumnName("amount")
                   .IsRequired();

            builder.Property(t => t.Description)
                   .HasColumnName("description")
                   .HasMaxLength(500);

            builder.Property(t => t.BalanceAfter)
                   .HasColumnName("balance_after")
                   .IsRequired();

            builder.Property(t => t.ReferenceId)
                   .HasColumnName("reference_id");

            builder.Property(t => t.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relation: 1 User -> nhiều AITransactions
            builder.HasOne(t => t.User)
                   .WithMany(u => u.AITransactions)
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
