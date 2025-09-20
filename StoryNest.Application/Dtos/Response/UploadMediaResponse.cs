using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class UploadMediaResponse
    {
        public List<PresignUrlResponse> Uploads { get; set; } = new();
    }
}
