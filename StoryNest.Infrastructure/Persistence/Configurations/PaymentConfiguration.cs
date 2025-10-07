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
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd()
                   .UseIdentityAlwaysColumn();

            builder.Property(p => p.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(p => p.SubscriptionId)
                   .HasColumnName("subscription_id")
                   .IsRequired();

            builder.Property(p => p.Amount)
                   .HasColumnName("amount")
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(p => p.Currency)
                   .HasColumnName("currency")
                   .HasMaxLength(10)
                   .IsRequired();

            builder.Property(p => p.Provider)
                   .HasColumnName("provider")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(p => p.ProviderTXN)
                   .HasColumnName("provider_txn")
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(p => p.Status)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(p => p.PaidAt)
                   .HasColumnName("paid_at");

            builder.Property(p => p.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(p => p.UpdatedAt)
                   .HasColumnName("updated_at");

            // Relation: 1 User -> nhiều Payments
            builder.HasOne(p => p.User)
                   .WithMany(u => u.Payments)
                   .HasForeignKey(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relation: 1 Subscription -> nhiều Payments
            builder.HasOne(p => p.Subscription)
                   .WithMany(s => s.Payments)
                   .HasForeignKey(p => p.SubscriptionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
