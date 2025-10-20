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
    public class StorySentimentAnalysisConfiguration : IEntityTypeConfiguration<StorySentimentAnalysis>
    {
        public void Configure(EntityTypeBuilder<StorySentimentAnalysis> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Score)
                   .HasColumnName("score")
                   .HasPrecision(4, 3);

            builder.Property(x => x.Magnitude)
                   .HasColumnName("magnitude")
                   .HasPrecision(6, 3);

            builder.Property(x => x.AnalyzedAt)
                   .HasColumnName("analyzed_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.AnalyzedText)
                   .HasColumnName("analyzed_text")
                   .HasColumnType("text");

            builder.Property(x => x.Source)
                   .HasColumnName("source")
                   .HasMaxLength(50);

            builder.Property(x => x.JobId)
                   .HasColumnName("job_id")
                   .HasMaxLength(100);

            builder.Property(x => x.ErrorMessage)
                   .HasColumnName("error_message")
                   .HasColumnType("text");

            builder.HasOne(x => x.Story)
                   .WithMany(s => s.SentimentAnalyses)
                   .HasForeignKey(x => x.StoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
