using AngleSharp;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.OpenAI
{
    public class OpenAIService : IOpenAIService
    {
        public readonly IConfiguration _configuration;

        public OpenAIService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<string> GenerateImageAsync(string content)
        {
            throw new NotImplementedException();
        }
    }
}
