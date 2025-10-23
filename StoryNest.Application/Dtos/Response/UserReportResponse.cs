using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class UserReportResponse
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public long ReportedId { get; set; }
        public long reporterId { get; set; }
        public int ReportedStoryId { get; set; }
        public int ReportedCommentId { get; set; }
        public UserBasicResponse ReportedUser { get; set; }
        public UserBasicResponse Reporter { get; set; }
        public string Reason { get; set; }
        public ReportType Type { get; set; }
        public ReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
