using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class GenerateCardImageResponse
    {
        public string ImageUrl { get; set; } = null!;
        public int CreditsUsed { get; set; }
    }
}
