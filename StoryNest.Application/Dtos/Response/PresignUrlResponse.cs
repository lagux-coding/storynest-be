using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class PresignUrlResponse
    {
        public string S3Url { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string MediaUrl { get; set; } = string.Empty;
    }
}
