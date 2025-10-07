using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IOpenAIService
    {
        Task<string> GenerateImageAsync(string content, long userId);
        Task<string> GenerateAudioAsync(string content, long userId);
    }
}
