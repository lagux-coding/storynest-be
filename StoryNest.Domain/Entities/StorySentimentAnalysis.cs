using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class StorySentimentAnalysis
    {
        public int Id { get; set; }

        // Story liên kết 
        public int StoryId { get; set; }
        public Story Story { get; set; } = default!;

        // Điểm sentiment từ Google NLP
        public float Score { get; set; }          // [-1, 1]
        public float Magnitude { get; set; }      // [0, ∞)

        // Text đã normalize (để trace lại)
        public string? AnalyzedText { get; set; }

        // Thông tin job
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
        public string? Source { get; set; } = "GoogleNLP"; // hoặc "Manual", "Retry", v.v.
        public string? JobId { get; set; }                 // nếu chạy theo batch / Quartz job

        // Cờ phân loại thêm (optional)
        public bool IsSuccessful { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }
}
