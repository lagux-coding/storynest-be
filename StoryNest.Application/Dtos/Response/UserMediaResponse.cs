using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class UserMediaResponse
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public MediaType MediaType { get; set; }
        public string MediaUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public UserMediaStatus Status { get; set; }
    }
}
